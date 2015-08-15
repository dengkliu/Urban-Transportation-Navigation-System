using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.NetworkAnalysis;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.SystemUI;

namespace trydotnet
{
    public partial class Form1 : Form
    {
        ClsPathFinder cls = new ClsPathFinder();
        IPolyline ipPolyResult;
        IPointCollection ippoints;
        IPolygon ipolygon;
        static int identifyflag = 0;
        static int modeflag = 1;
        IFeatureClass mainroad; IFeatureLayer mainroadlayer;
        IFeatureClass secondroad; IFeatureLayer secondroadlayer;
        IFeatureClass waterregion; IFeatureLayer waterregionlayer;
        IFeatureClass supermarket; IFeatureLayer supermarketlayer;
        IFeatureClass address; IFeatureLayer addresslayer;
        IFeatureClass hotel; IFeatureLayer hotellayer;
        IFeatureClass trainstation; IFeatureLayer trainstationlayer;
        IFeatureClass bank; IFeatureLayer banklayer;
        IFeatureClass sight; IFeatureLayer sightlayer;
        IFeatureClass busstation; IFeatureLayer busstationlayer;
        IFeatureClass government; IFeatureLayer governmentlayer;
        IFeatureClass hospital; IFeatureLayer hospitallayer;
        IFeatureClass recreation; IFeatureLayer recreationlayer;
        IFeatureClass railway; IFeatureLayer railwaylayer;
        System.Collections.ArrayList barrierarray1 = new System.Collections.ArrayList();
        System.Collections.ArrayList barrierarray2 = new System.Collections.ArrayList();
        public Form1()
        {
            InitializeComponent();
            this.axMapControl1.OnMouseDown += new IMapControlEvents2_Ax_OnMouseDownEventHandler(axMapControl1_OnMouseDown);
            this.axMapControl1.OnDoubleClick += new IMapControlEvents2_Ax_OnDoubleClickEventHandler(axMapControl1_OnDoubleClick);
            this.axMapControl1.OnExtentUpdated += new IMapControlEvents2_Ax_OnExtentUpdatedEventHandler(axMapControl1_OnExtentUpdated);
            this.axMapControl2.OnMouseMove += new IMapControlEvents2_Ax_OnMouseMoveEventHandler(axMapControl2_OnMouseMove);
            this.axMapControl2.OnMouseDown += new IMapControlEvents2_Ax_OnMouseDownEventHandler(axMapControl2_OnMouseDown);
            setupmap();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            IMapDocument imapDocument = new MapDocumentClass();
            imapDocument.Open(@"C:\Users\huty\Desktop\武汉实习数据\wuhan.mxd", "");
            for (int i = 0; i <= imapDocument.MapCount - 1; i++)
            {
                axMapControl1.Map = imapDocument.get_Map(i);//axMapControl1为MapControl的自动对象
            }
            IGraphicsContainer pgraghicscontainer = axMapControl1.Map as IGraphicsContainer;
            IActiveView pactiveview = pgraghicscontainer as IActiveView;
            axMapControl1.Extent = pactiveview.FullExtent;
            pactiveview.Refresh();
            axMapControl2.AddLayer(mainroadlayer);
            axMapControl2.AddLayer(secondroadlayer);
            axMapControl2.AddLayer(waterregionlayer);
            double scale = axMapControl1.ActiveView.FocusMap.MapScale;
            string a = scale.ToString("#0.00");
            this.scaletextbox.Text = "1:" + a;
            clsload();
            this.select.Checked = true;
        }

        private void axMapControl2_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if (axMapControl2.Map.LayerCount > 0)
            {
                if (e.button == 1)
                {
                    IPoint ppoint = new PointClass();
                    ppoint.PutCoords(e.mapX, e.mapY);
                    axMapControl1.CenterAt(ppoint);
                    axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                }
                else if (e.button == 2)
                {
                    IEnvelope penv = axMapControl2.TrackRectangle();
                    axMapControl2.Extent = penv;
                    axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                }
            }
        }

        private void axMapControl2_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            if (e.button == 1)
            {
                IPoint ppoint = new PointClass();
                ppoint.PutCoords(e.mapX, e.mapY);
                axMapControl1.CenterAt(ppoint);
                axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            }
        }

        //private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        //{
        //    IPoint ipNew = axMapControl1.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);
        //    this.ribbonBar1.Text = "当前在地图上的坐标为  X：" + Convert.ToString(e.x) + "   Y:" + Convert.ToString(e.y);
        //}

        private void axMapControl1_OnDoubleClick(object sender, IMapControlEvents2_OnDoubleClickEvent e)
        {
            if (areaprocess.Checked)
            {
                IGraphicsContainer pgraghicscontainer = axMapControl1.Map as IGraphicsContainer;
                IActiveView pactiveview = pgraghicscontainer as ESRI.ArcGIS.Carto.IActiveView;
                IPoint ipNew;
                if (ippoints == null)
                    ippoints = new MultipointClass();
                ipNew = axMapControl1.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);
                object o = Type.Missing;
                ippoints.AddPoint(ipNew, ref o, ref o);
                createpolygon(ippoints);
                if (ipPolyResult != null)
                {
                    drawpolygon(ipPolyResult);
                }
                drawpoints(ippoints);
            }
        }

        private void axMapControl1_OnExtentUpdated(object sender, IMapControlEvents2_OnExtentUpdatedEvent e)
        {
            double scale = axMapControl1.ActiveView.FocusMap.MapScale;
            string a = scale.ToString("#0.00");
            this.scaletextbox.Text = "1:" + a;

            IEnvelope penvelope = (IEnvelope)e.newEnvelope;

            IGraphicsContainer pgraghicscontainer = axMapControl2.Map as IGraphicsContainer;
            IActiveView pactiveview = pgraghicscontainer as IActiveView;

            pgraghicscontainer.DeleteAllElements();
            IRectangleElement prectangle = new RectangleElementClass();
            IElement pelement = prectangle as IElement;
            pelement.Geometry = penvelope;

            IRgbColor pcolor1 = new RgbColorClass();
            pcolor1.Red = 255; pcolor1.Green = 0; pcolor1.Blue = 0; pcolor1.Transparency = 0;
            IRgbColor pcolor2 = new RgbColorClass();
            pcolor2.Red = 255; pcolor2.Green = 0; pcolor2.Blue = 0; pcolor2.Transparency = 255;

            ISimpleFillSymbol simplefillsymbol = new SimpleFillSymbolClass();
            simplefillsymbol.Color = pcolor1;
            simplefillsymbol.Outline.Color = pcolor2;
            simplefillsymbol.Outline.Width = 3;
            IFillShapeElement pfillshapefile = pelement as IFillShapeElement;
            pfillshapefile.Symbol = simplefillsymbol;
            pgraghicscontainer.AddElement((IElement)pfillshapefile, 0);
            axMapControl2.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        private void zoomin_Click(object sender, EventArgs e)
        {
            zoomout.Checked = false;
            pan.Checked = false;
            findpath.Checked = false;
            startcalculating.Checked = false;
            select.Checked = false;
            distanceprocess.Checked = false;
            ICommand pcommand;
            pcommand = new ControlsMapZoomInTool();
            pcommand.OnCreate(axMapControl1.Object);
            axMapControl1.CurrentTool = pcommand as ITool;
        }

        private void zoomout_Click(object sender, EventArgs e)
        {
            zoomin.Checked = false;
            pan.Checked = false;
            findpath.Checked = false;
            startcalculating.Checked = false;
            select.Checked = false;
            distanceprocess.Checked = false;
            ICommand pcommand;
            pcommand = new ControlsMapZoomOutTool();
            pcommand.OnCreate(axMapControl1.Object);
            axMapControl1.CurrentTool = pcommand as ITool;
        }

        private void viewentire_Click(object sender, EventArgs e)
        {
            IMapLayers mapLayers = axMapControl1.Map as IMapLayers;
            mapLayers.DeleteLayer(trainstationlayer); mapLayers.DeleteLayer(mainroadlayer); mapLayers.DeleteLayer(secondroadlayer);
            mapLayers.DeleteLayer(busstationlayer); mapLayers.DeleteLayer(waterregionlayer); mapLayers.DeleteLayer(railwaylayer);
            mapLayers.DeleteLayer(supermarketlayer); mapLayers.DeleteLayer(banklayer); mapLayers.DeleteLayer(governmentlayer);
            mapLayers.DeleteLayer(hospitallayer); mapLayers.DeleteLayer(recreationlayer); mapLayers.DeleteLayer(hotellayer);
            sightbutton.Checked = false; trainstationbutton.Checked = false; supermarketbutton.Checked = false; bankbutton.Checked = false;
            governmentbutton.Checked = false; hospitalbutton.Checked = false; secondroadbutton.Checked = false; waterregionbutton.Checked = false;
            railwaybutton.Checked = false; recreationbutton.Checked = false; hotelbutton.Checked = false; sightbutton.Checked = false;
            mainroadbutton.Checked = false; busstationbutton.Checked = false;
            mapLayers.DeleteLayer(sightlayer); mapLayers.DeleteLayer(addresslayer);
            zoomin.Checked = false;
            zoomout.Checked = false;
            pan.Checked = false;
            findpath.Checked = false;
            startcalculating.Checked = false;
            select.Checked = false;
            distanceprocess.Checked = false;
            IGraphicsContainer pgraghicscontainer = axMapControl1.Map as IGraphicsContainer;
            IActiveView pactiveview = pgraghicscontainer as IActiveView;
            axMapControl1.Extent = pactiveview.FullExtent;
            axMapControl1.CurrentTool = null;
        }

        private void pan_Click(object sender, EventArgs e)
        {
            zoomin.Checked = false;
            zoomout.Checked = false;
            findpath.Checked = false;
            startcalculating.Checked = false;
            select.Checked = false;
            distanceprocess.Checked = false;
            ICommand pcommand = new ControlsMapPanTool();
            pcommand.OnCreate(axMapControl1.Object);
            axMapControl1.CurrentTool = pcommand as ITool;
        }

        #region draw//绘制图像
        private void drawpathline(IPolyline ipolyline)
        {
            IGraphicsContainer pgraghicscontainer = axMapControl1.Map as IGraphicsContainer;
            IActiveView pactiveview = pgraghicscontainer as ESRI.ArcGIS.Carto.IActiveView;
            //设定路径颜色
            IRgbColor pcolor = new RgbColorClass();
            pcolor.Red = 255;
            pcolor.Green = 180;
            pcolor.Blue = 30;
            pcolor.Transparency = 100;
            //设置线格式
            ISimpleLineSymbol simpleLineSymbol = new SimpleLineSymbolClass();
            simpleLineSymbol.Color = pcolor;
            simpleLineSymbol.Width = 2;
            //绘出路径线
            IScreenDisplay screenDisplay = pactiveview.ScreenDisplay;
            ISymbol symbol = (ISymbol)simpleLineSymbol;
            screenDisplay.StartDrawing(screenDisplay.hDC, System.Convert.ToInt16(esriScreenCache.esriNoScreenCache));
            screenDisplay.SetSymbol(symbol);
            screenDisplay.DrawPolyline(ipolyline);
            screenDisplay.FinishDrawing();
        }

        private void drawpathline2(IPolyline ipolyline)
        {
            IGraphicsContainer pgraghicscontainer = axMapControl1.Map as IGraphicsContainer;
            IActiveView pactiveview = pgraghicscontainer as ESRI.ArcGIS.Carto.IActiveView;
            //设定路径颜色
            IRgbColor pcolor = new RgbColorClass();
            pcolor.Red = 0;
            pcolor.Green = 0;
            pcolor.Blue = 255;
            pcolor.Transparency = 100;
            //设置线格式
            ISimpleLineSymbol simpleLineSymbol = new SimpleLineSymbolClass();
            simpleLineSymbol.Color = pcolor;
            simpleLineSymbol.Width = 2;
            //绘出路径线
            IScreenDisplay screenDisplay = pactiveview.ScreenDisplay;
            ISymbol symbol = (ISymbol)simpleLineSymbol;
            screenDisplay.StartDrawing(screenDisplay.hDC, System.Convert.ToInt16(esriScreenCache.esriNoScreenCache));
            screenDisplay.SetSymbol(symbol);
            screenDisplay.DrawPolyline(ipolyline);
            screenDisplay.FinishDrawing();
        }

        private void drawpoints(IPointCollection ippoints)
        {
            IGraphicsContainer pgraghicscontainer = axMapControl1.Map as IGraphicsContainer;
            IActiveView pactiveview = pgraghicscontainer as ESRI.ArcGIS.Carto.IActiveView;
            //设定路径颜色
            IRgbColor pcolor = new RgbColorClass();
            pcolor.Red = 255;
            pcolor.Green = 0;
            pcolor.Blue = 0;
            pcolor.Transparency = 100;
            //设置线格式
            ISimpleMarkerSymbol simpleMarkerSymbol = new SimpleMarkerSymbolClass();
            simpleMarkerSymbol.Color = pcolor;
            simpleMarkerSymbol.Size = 4;
            //绘出路径线
            IScreenDisplay screenDisplay = pactiveview.ScreenDisplay;
            ISymbol symbol = (ISymbol)simpleMarkerSymbol;
            screenDisplay.StartDrawing(screenDisplay.hDC, System.Convert.ToInt16(esriScreenCache.esriNoScreenCache));
            screenDisplay.SetSymbol(symbol);
            int i;
            for (i = 0; i < ippoints.PointCount; i++)
            {
                screenDisplay.DrawPoint(ippoints.get_Point(i));
            }
            screenDisplay.FinishDrawing();
        }

        private void drawpolygon(IPolyline ipolyline)
        {
            IGraphicsContainer pgraghicscontainer = axMapControl1.Map as IGraphicsContainer;
            IActiveView pactiveview = pgraghicscontainer as ESRI.ArcGIS.Carto.IActiveView;
            //设定路径颜色
            IRgbColor pcolor = new RgbColorClass();
            pcolor.Red = 255;
            pcolor.Green = 180;
            pcolor.Blue = 30;
            pcolor.Transparency = 100;
            //设置线格式
            ISimpleLineSymbol simpleLineSymbol = new SimpleLineSymbolClass();
            simpleLineSymbol.Color = pcolor;
            simpleLineSymbol.Width = 2;
            //绘出路径线
            IScreenDisplay screenDisplay = pactiveview.ScreenDisplay;
            ISymbol symbol = (ISymbol)simpleLineSymbol;
            screenDisplay.StartDrawing(screenDisplay.hDC, System.Convert.ToInt16(esriScreenCache.esriNoScreenCache));
            screenDisplay.SetSymbol(symbol);
            screenDisplay.DrawPolyline(ipolyline);
            screenDisplay.FinishDrawing();

        }
        #endregion

        private void clsload()
        {
            IMap ipmap;
            //打开工作空间
            Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
            IWorkspace workspace = workspaceFactory.OpenFromFile(@"C:\Users\huty\Desktop\武汉实习数据\道路.gdb", 0);
            IFeatureWorkspace featureworkspace = (IFeatureWorkspace)workspace;
            //查找数据集
            IEnumDataset pEnumDataset = workspace.get_Datasets(ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTAny);
            IDataset pDataset = pEnumDataset.Next();
            IFeatureDataset Featuredataset = featureworkspace.OpenFeatureDataset(pDataset.Name);
            //初始化查找最短路径类
            ipmap = this.axMapControl1.ActiveView.FocusMap;
            cls.SetOrGetMap = ipmap;
            cls.OpenFeatureDatasetNetwork(Featuredataset,barrierarray1,barrierarray2,0);
        }

        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if (findpath.Checked)
            {
                IPoint ipNew;
                if (ippoints == null)
                    ippoints = new MultipointClass();
                ipNew = axMapControl1.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);
                object o = Type.Missing;
                ippoints.AddPoint(ipNew, ref o, ref o);
                cls.StopPoints = ippoints;
                drawpoints(ippoints);
            }
            if (findpath2.Checked)
            {
                IPoint ipNew;
                if (ippoints == null)
                    ippoints = new MultipointClass();
                ipNew = axMapControl1.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);
                object o = Type.Missing;
                ippoints.AddPoint(ipNew, ref o, ref o);
                cls.StopPoints = ippoints;
                drawpoints(ippoints);
            }
            if (distanceprocess.Checked)
            {
                IPoint ipNew;
                if (ippoints == null)
                    ippoints = new MultipointClass();
                ipNew = axMapControl1.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);
                object o = Type.Missing;
                ippoints.AddPoint(ipNew, ref o, ref o);
                createpolyline(ippoints);
                if (ipPolyResult != null)
                    drawpathline(ipPolyResult);
                drawpoints(ippoints);
            }
            if (areaprocess.Checked)
            {
                IGraphicsContainer pgraghicscontainer = axMapControl1.Map as IGraphicsContainer;
                IActiveView pactiveview = pgraghicscontainer as ESRI.ArcGIS.Carto.IActiveView;
                IPoint ipNew;
                if (ippoints == null)
                    ippoints = new MultipointClass();
                ipNew = axMapControl1.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);
                object o = Type.Missing;
                ippoints.AddPoint(ipNew, ref o, ref o);
                createpolyline(ippoints);
                if (ipPolyResult != null)
                {
                    drawpathline(ipPolyResult);
                }
                drawpoints(ippoints);
            }
            if (identify.Checked)
            {
                int m_px = e.x;
                int m_py = e.y;
                selectfeature(m_px, m_py);
            }
            if (select.Checked)
            {
                int m_px = e.x;
                int m_py = e.y;
                selectfeature(m_px, m_py);
            }
        }

        private void select_Click(object sender, EventArgs e)
        {
            zoomin.Checked = false;
            zoomout.Checked = false;
            pan.Checked = false;
            findpath.Checked = false;
            startcalculating.Checked = false;
            distanceprocess.Checked = false;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;
            axMapControl1.CurrentTool = null;
        }

        private void distanceprocess_Click_1(object sender, EventArgs e)
        {
            zoomin.Checked = false;
            zoomout.Checked = false;
            pan.Checked = false;
            findpath.Checked = false;
            startcalculating.Checked = false;
            select.Checked = false;
            areaprocess.Checked = false;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
        }

        private void areaprocess_Click(object sender, EventArgs e)
        {
            zoomin.Checked = false;
            zoomout.Checked = false;
            pan.Checked = false;
            findpath.Checked = false;
            startcalculating.Checked = false;
            select.Checked = false;
            distanceprocess.Checked = false;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
        }

        private void process_Click(object sender, EventArgs e)
        {
            if (distanceprocess.Checked)
            {
                double scale = axMapControl1.ActiveView.FocusMap.MapScale;
                if (ipPolyResult != null)
                {

                    double length = ipPolyResult.Length;
                    double distance1 = length * 100000;
                    double distance2 = length * 100;
                    string distancestring1 = distance1.ToString("#0.00");
                    string distancestring2 = distance2.ToString("#0.00");
                    if (distance1 < 1000)
                        MessageBox.Show("总路径长度为" + distancestring1 + "米。");
                    else MessageBox.Show("总路径长度为" + distancestring2 + "千米。");
                }
                ipPolyResult.SetEmpty();
                ippoints.RemovePoints(0, ippoints.PointCount);
                axMapControl1.Refresh();
                this.distanceprocess.Checked = false;
            }
            if (areaprocess.Checked)
            {
                try
                {
                    IArea pArea = (IArea)ipolygon;
                    double dArea = Math.Abs(pArea.Area);
                    double area1 = dArea * 1000000000;
                    double area2 = dArea * 1000;
                    string areastring1 = area1.ToString("#0.00");
                    string areastring2 = area2.ToString("#0.00");
                    if (area1 < 1000000)
                        MessageBox.Show("选择区域面积为" + areastring1 + "平方米。");
                    else MessageBox.Show("选择区域面积为" + areastring2 + "平方公里。");
                }
                catch (Exception)
                {
                    MessageBox.Show("区域不闭合或者其他错误！！");
                }
                finally
                {
                    ipPolyResult.SetEmpty();
                    ippoints.RemovePoints(0, ippoints.PointCount);
                    axMapControl1.Refresh();
                    this.areaprocess.Checked = false;
                }
            }

        }

        private void createpolyline(IPointCollection ippoints)
        {
            ISegmentCollection ppath = new PathClass();
            IGeometryCollection ppolyline = new PolylineClass();
            if (ippoints.PointCount >= 2)
            {
                int i;
                for (i = 0; i < ippoints.PointCount - 1; i++)
                {
                    ILine pline = new LineClass();
                    pline.PutCoords(ippoints.get_Point(i), ippoints.get_Point(i + 1));
                    ISegment psegment = pline as ISegment;
                    object o = Type.Missing;
                    ppath.AddSegment(psegment, ref o, ref o);
                    ppolyline.AddGeometry(ppath as IGeometry, ref o, ref o);
                }
                ipPolyResult = ppolyline as IPolyline;
            }
        }

        private void createpolygon(IPointCollection ippoints)
        {
            ISegmentCollection ppath = new PathClass();
            IGeometryCollection ppolyline = new PolylineClass();
            if (ippoints.PointCount >= 3)
            {
                int i;
                object o = Type.Missing;
                if (ippoints.PointCount >= 4)
                {
                    ippoints.RemovePoints(ippoints.PointCount - 2, 1);
                }
                ippoints.AddPoint(ippoints.get_Point(0));
                for (i = 0; i < ippoints.PointCount - 1; i++)
                {
                    ILine pline = new LineClass();
                    pline.PutCoords(ippoints.get_Point(i), ippoints.get_Point(i + 1));
                    ISegment psegment = pline as ISegment;

                    ppath.AddSegment(psegment, ref o, ref o);
                    ppolyline.AddGeometry(ppath as IGeometry, ref o, ref o);
                }
                ipPolyResult = ppolyline as IPolyline;
                ISegmentCollection pRing = new RingClass();
                IGeometryCollection pGeometryColl = new PolygonClass();
                for (int j = 0; j < ppolyline.GeometryCount; j++)
                {
                    pRing.AddSegmentCollection(ppolyline.get_Geometry(j) as ISegmentCollection);
                    pGeometryColl.AddGeometry(pRing as IGeometry, ref o, ref o);
                }
                ipolygon = pGeometryColl as IPolygon;
            }
        }

        private void setupmap()
        {
            IWorkspaceFactory workspaceFactory = new ShapefileWorkspaceFactory();
            IWorkspace workspace = workspaceFactory.OpenFromFile(@"C:\Users\huty\Desktop\武汉实习数据", 0);
            IFeatureWorkspace featureworkspace = (IFeatureWorkspace)workspace;

            mainroad = featureworkspace.OpenFeatureClass("主干道.shp");
            mainroadlayer = new FeatureLayerClass();
            mainroadlayer.FeatureClass = mainroad;
            mainroadlayer.Name = "mainroad";

            secondroad = featureworkspace.OpenFeatureClass("次干道.shp");
            secondroadlayer = new FeatureLayerClass();
            secondroadlayer.FeatureClass = secondroad;
            secondroadlayer.Name = "secondroad";

            waterregion = featureworkspace.OpenFeatureClass("水域.shp");
            waterregionlayer = new FeatureLayerClass();
            waterregionlayer.FeatureClass = waterregion;
            waterregionlayer.Name = "waterregion";

            supermarket = featureworkspace.OpenFeatureClass("超市.shp");
            supermarketlayer = new FeatureLayerClass();
            supermarketlayer.FeatureClass = supermarket;
            supermarketlayer.Name = "supermarket";

            address = featureworkspace.OpenFeatureClass("地名.shp");
            addresslayer = new FeatureLayerClass();
            addresslayer.FeatureClass = address;
            addresslayer.Name = "address";

            hotel = featureworkspace.OpenFeatureClass("饭店.shp");
            hotellayer = new FeatureLayerClass();
            hotellayer.FeatureClass = hotel;
            hotellayer.Name = "hotel";

            trainstation = featureworkspace.OpenFeatureClass("火车站.shp");
            trainstationlayer = new FeatureLayerClass();
            trainstationlayer.FeatureClass = trainstation;
            trainstationlayer.Name = "trainstation";

            bank = featureworkspace.OpenFeatureClass("金融银行.shp");
            banklayer = new FeatureLayerClass();
            banklayer.FeatureClass = bank;
            banklayer.Name = "bank";

            sight = featureworkspace.OpenFeatureClass("景点.shp");
            sightlayer = new FeatureLayerClass();
            sightlayer.FeatureClass = sight;
            sightlayer.Name = "sight";

            busstation = featureworkspace.OpenFeatureClass("汽车站.shp");
            busstationlayer = new FeatureLayerClass();
            busstationlayer.FeatureClass = busstation;
            busstationlayer.Name = "busstation";

            government = featureworkspace.OpenFeatureClass("区政府.shp");
            governmentlayer = new FeatureLayerClass();
            governmentlayer.FeatureClass = government;
            governmentlayer.Name = "government";

            hospital = featureworkspace.OpenFeatureClass("医院.shp");
            hospitallayer = new FeatureLayerClass();
            hospitallayer.FeatureClass = hospital;
            hospitallayer.Name = "hospital";

            recreation = featureworkspace.OpenFeatureClass("娱乐场所.shp");
            recreationlayer = new FeatureLayerClass();
            recreationlayer.FeatureClass = recreation;
            recreationlayer.Name = "recreation";

            railway = featureworkspace.OpenFeatureClass("铁路.shp");
            railwaylayer = new FeatureLayerClass();
            railwaylayer.FeatureClass = railway;
            railwaylayer.Name = "railway";
        }

        private void identify_Click(object sender, EventArgs e)
        {
            if (identifyflag == 0)
            {
                ICommand pcommand = new ControlsMapIdentifyTool();
                pcommand.OnCreate(axMapControl1.Object);
                axMapControl1.CurrentTool = pcommand as ITool;
                identify.Checked = true;
                identifyflag = 1;
            }
            else
            {
                axMapControl1.CurrentTool = null;
                identify.Checked = false;
                identifyflag = 0;
            }
        }

        private void findpath_Click(object sender, EventArgs e)
        {
            zoomin.Checked = false;
            zoomout.Checked = false;
            pan.Checked = false;
            select.Checked = false;
            distanceprocess.Checked = false;
            startcalculating.Enabled = true;
            axMapControl1.CurrentTool = null;
            this.checkBox1.Enabled = false;
            this.checkBox2.Enabled = false;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            if (checkBox1.Checked)
            {
                if (ippoints == null)
                    ippoints = new MultipointClass();
                IPoint ipoint = axMapControl1.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(579, 390);
                object o = Type.Missing;
                ippoints.AddPoint(ipoint,ref o,ref o);
            }
        }

        private void startcalculating_Click(object sender, EventArgs e)
        {
            zoomin.Checked = false;
            zoomout.Checked = false;
            pan.Checked = false;
            distanceprocess.Checked = false;
            if (modeflag == 2)
                rainmode2();
            cls.SolvePath("长度");
            if (cls.PathPolyLine())
            {
                ipPolyResult = cls.m_ipPolyline;
                drawpathline(ipPolyResult);
                double length = cls.PathCost;
                double distance = length / 1000;
                string distancestring1 = length.ToString("#0.00");
                string distancestring2 = distance.ToString("#0.00");
                if (length < 1000)
                    MessageBox.Show("总路径长度为" + distancestring1 + "米。");
                else MessageBox.Show("总路径长度为" + distancestring2 + "千米。");
            }
            else MessageBox.Show("选取的点超过范围或路径不连通!");
        }

        private void stopcalculating_Click(object sender, EventArgs e)
        {
            zoomin.Checked = false;
            zoomout.Checked = false;
            pan.Checked = false;
            findpath.Checked = false;
            startcalculating.Checked = false;
            distanceprocess.Checked = false;
            select.Checked = false;
            startcalculating.Enabled = false;
            this.checkBox1.Enabled = true;
            this.checkBox2.Enabled = true;
            this.checkBox1.Checked = false;
            this.checkBox2.Checked = false;
            if (ippoints != null)
            {
                ippoints.RemovePoints(0, ippoints.PointCount);
                if(ipPolyResult!=null)
                    ipPolyResult.SetEmpty();
            }
            axMapControl1.ActiveView.Refresh();
        }

        private void choose_Click(object sender, EventArgs e)
        {
            switch (this.choosemodecombobox.SelectedIndex)
            {
                case 0: { this.shortestribbonBar.Visible = true; this.quickestribbonBar.Visible = false; } break;
                case 1: { this.shortestribbonBar.Visible = false; this.quickestribbonBar.Visible = true; } break;
                default: { this.shortestribbonBar.Visible = false; this.quickestribbonBar.Visible = false; } break;
            }
        }

        private void findpath2_Click(object sender, EventArgs e)
        {
            zoomin.Checked = false;
            zoomout.Checked = false;
            pan.Checked = false;
            select.Checked = false;
            distanceprocess.Checked = false;
            startcalculating2.Enabled = true;
            axMapControl1.CurrentTool = null;
            this.checkBox1.Enabled = false;
            this.checkBox2.Enabled = false;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            if (checkBox2.Checked)
            {
                if (ippoints == null)
                    ippoints = new MultipointClass();
                IPoint ipoint = axMapControl1.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(579, 390);
                object o = Type.Missing;
                ippoints.AddPoint(ipoint, ref o, ref o);
            }
        }

        private void startcalculating2_Click(object sender, EventArgs e)
        {
            zoomin.Checked = false;
            zoomout.Checked = false;
            pan.Checked = false;
            distanceprocess.Checked = false;
            if (modeflag == 2)
                rainmode2();
            cls.SolvePath("通行时间");
            if (cls.PathPolyLine())
            {
                ipPolyResult = cls.m_ipPolyline;
                drawpathline(ipPolyResult);
                int time = (int)cls.PathCost;
                int hour = 0, minute = 0, hour2 = 0, minute2 = 0;
                minute = time / 60; minute2 = time % 60;
                hour = minute / 60; hour2 = minute % 60;
                if (time < 60)
                {
                    MessageBox.Show("路径一共用时" + Convert.ToString(time) + "秒。");
                }
                if (time >= 60 && time < 3600)
                {
                    MessageBox.Show("路径一共用时" + Convert.ToString(minute) + "分" + Convert.ToString(minute2) + "秒。");
                }
                if (time >= 3600)
                {
                    MessageBox.Show("路径一共用时" + Convert.ToString(hour) + "小时" + Convert.ToString(hour2) + "分" + Convert.ToString(minute2) + "秒");
                }
            }
            else MessageBox.Show("选取的点超过范围或路径不连通!");
        }

        private void stopcalculating2_Click(object sender, EventArgs e)
        {
            zoomin.Checked = false;
            zoomout.Checked = false;
            pan.Checked = false;
            findpath2.Checked = false;
            startcalculating2.Checked = false;
            distanceprocess.Checked = false;
            select.Checked = false;
            startcalculating2.Enabled = false;
            this.checkBox1.Enabled = true;
            this.checkBox2.Enabled = true;
            this.checkBox1.Checked = false;
            this.checkBox2.Checked = false;
            if (ippoints != null)
            {
                ippoints.RemovePoints(0, ippoints.PointCount);
                if (ipPolyResult != null)
                    ipPolyResult.SetEmpty();
            }
            axMapControl1.ActiveView.Refresh();
        }

        private void zoomtofeature_Click(object sender, EventArgs e)
        {
            IMap imap = axMapControl1.Map;
            ICommand pCommand = new ControlsZoomToSelectedCommandClass();
            pCommand.OnCreate(axMapControl1.Object);
            pCommand.OnClick();
        }

        #region selectandsearch
        private void selectfeature(int x, int y)
        {
            IMap imap = axMapControl1.Map;
            imap.ClearSelection();
            int errorflag = 0;
            sightbutton.Checked = false; trainstationbutton.Checked = false; supermarketbutton.Checked = false; bankbutton.Checked = false;
            governmentbutton.Checked = false; hospitalbutton.Checked = false; secondroadbutton.Checked = false; waterregionbutton.Checked = false;
            railwaybutton.Checked = false; recreationbutton.Checked = false; hotelbutton.Checked = false; sightbutton.Checked = false;
            mainroadbutton.Checked = false; busstationbutton.Checked = false;
            IMapLayers mapLayers = axMapControl1.Map as IMapLayers;
            mapLayers.DeleteLayer(trainstationlayer); mapLayers.DeleteLayer(mainroadlayer); mapLayers.DeleteLayer(secondroadlayer);
            mapLayers.DeleteLayer(busstationlayer); mapLayers.DeleteLayer(waterregionlayer); mapLayers.DeleteLayer(railwaylayer);
            mapLayers.DeleteLayer(supermarketlayer); mapLayers.DeleteLayer(banklayer); mapLayers.DeleteLayer(governmentlayer);
            mapLayers.DeleteLayer(hospitallayer); mapLayers.DeleteLayer(recreationlayer); mapLayers.DeleteLayer(hotellayer);
            mapLayers.DeleteLayer(sightlayer); mapLayers.DeleteLayer(addresslayer);
            IActiveView pActiveView = imap as IActiveView;
            IPoint pPoint = pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            //Use a 4 pixel buffer around the cursor for feature search
            double length;
            length = ConvertPixelsToMapUnits(pActiveView, 4);
            ITopologicalOperator pTopo = pPoint as ITopologicalOperator;
            IGeometry pBuffer = pTopo.Buffer(length);//建立4个地图单位的缓冲区
            IGeometry pGeometry = pBuffer.Envelope;//确定鼠标周围隐藏的选择框
            //新建一个空间约束器
            ISpatialFilter pSpatialFilter;
            IQueryFilter pFilter;
            //设置查询约束条件
            pSpatialFilter = new SpatialFilter();
            pSpatialFilter.Geometry = pGeometry;
            try
            {
                IFeatureLayer pFeatureLayer = mainroadlayer as IFeatureLayer;
                IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                switch (pFeatureClass.ShapeType)
                {
                    case esriGeometryType.esriGeometryPoint:
                        pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                        break;
                    case esriGeometryType.esriGeometryPolyline:
                        pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
                        break;
                    case esriGeometryType.esriGeometryPolygon:
                        pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        break;
                    default:
                        break;
                }
                pSpatialFilter.GeometryField = pFeatureClass.ShapeFieldName;
                pFilter = pSpatialFilter;
                //Do the Search 从图层中查询出满足约束条件的元素
                IFeatureCursor pCursor = mainroadlayer.Search(pFilter, false);
                //select
                IFeature ifeature = pCursor.NextFeature();
                if (ifeature == null) throw new Exception();
                while (ifeature != null)
                {
                    imap.SelectFeature(pFeatureLayer, ifeature);
                    ifeature = pCursor.NextFeature();
                }
                axMapControl1.AddLayer(mainroadlayer);
                this.mainroadbutton.Checked = true;
                imap.SelectFeature(mainroadlayer, ifeature);
            }
            catch (Exception)
            {
                errorflag++;
                try
                {
                    IFeatureLayer pFeatureLayer = waterregionlayer as IFeatureLayer;
                    IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                    switch (pFeatureClass.ShapeType)
                    {
                        case esriGeometryType.esriGeometryPoint:
                            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                            break;
                        case esriGeometryType.esriGeometryPolyline:
                            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
                            break;
                        case esriGeometryType.esriGeometryPolygon:
                            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                            break;
                        default:
                            break;
                    }
                    pSpatialFilter.GeometryField = pFeatureClass.ShapeFieldName;
                    pFilter = pSpatialFilter;
                    //Do the Search 从图层中查询出满足约束条件的元素
                    IFeatureCursor pCursor = waterregionlayer.Search(pFilter, false);
                    //select
                    IFeature ifeature = pCursor.NextFeature();
                    if (ifeature == null) throw new Exception();
                    while (ifeature != null)
                    {
                        imap.SelectFeature(pFeatureLayer, ifeature);
                        ifeature = pCursor.NextFeature();
                    }
                    axMapControl1.AddLayer(waterregionlayer);
                    this.waterregionbutton.Checked = true;
                    imap.SelectFeature(waterregionlayer, ifeature);
                }
                catch (Exception)
                {
                    errorflag++;
                    try
                    {
                        IFeatureLayer pFeatureLayer = secondroadlayer as IFeatureLayer;
                        IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                        switch (pFeatureClass.ShapeType)
                        {
                            case esriGeometryType.esriGeometryPoint:
                                pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                                break;
                            case esriGeometryType.esriGeometryPolyline:
                                pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
                                break;
                            case esriGeometryType.esriGeometryPolygon:
                                pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                                break;
                            default:
                                break;
                        }
                        pSpatialFilter.GeometryField = pFeatureClass.ShapeFieldName;
                        pFilter = pSpatialFilter;
                        //Do the Search 从图层中查询出满足约束条件的元素
                        IFeatureCursor pCursor = secondroadlayer.Search(pFilter, false);
                        //select
                        IFeature ifeature = pCursor.NextFeature();
                        if (ifeature == null) throw new Exception();
                        while (ifeature != null)
                        {
                            imap.SelectFeature(pFeatureLayer, ifeature);
                            ifeature = pCursor.NextFeature();
                        }
                        axMapControl1.AddLayer(secondroadlayer);
                        this.secondroadbutton.Checked = true;
                        imap.SelectFeature(secondroadlayer, ifeature);
                    }
                    catch (Exception)
                    {
                        errorflag++;
                        try
                        {
                            IFeatureLayer pFeatureLayer = addresslayer as IFeatureLayer;
                            IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                            switch (pFeatureClass.ShapeType)
                            {
                                case esriGeometryType.esriGeometryPoint:
                                    pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                                    break;
                                case esriGeometryType.esriGeometryPolyline:
                                    pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
                                    break;
                                case esriGeometryType.esriGeometryPolygon:
                                    pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                                    break;
                                default:
                                    break;
                            }
                            pSpatialFilter.GeometryField = pFeatureClass.ShapeFieldName;
                            pFilter = pSpatialFilter;
                            //Do the Search 从图层中查询出满足约束条件的元素
                            IFeatureCursor pCursor = addresslayer.Search(pFilter, false);
                            //select
                            IFeature ifeature = pCursor.NextFeature();
                            if (ifeature == null) throw new Exception();
                            while (ifeature != null)
                            {
                                imap.SelectFeature(pFeatureLayer, ifeature);
                                ifeature = pCursor.NextFeature();
                            }
                            axMapControl1.AddLayer(addresslayer);
                            this.addressbutton.Checked = true;
                            imap.SelectFeature(addresslayer, ifeature);
                        }
                        catch (Exception)
                        {
                            errorflag++;
                            try
                            {
                                IFeatureLayer pFeatureLayer = trainstationlayer as IFeatureLayer;
                                IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                                switch (pFeatureClass.ShapeType)
                                {
                                    case esriGeometryType.esriGeometryPoint:
                                        pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                                        break;
                                    case esriGeometryType.esriGeometryPolyline:
                                        pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
                                        break;
                                    case esriGeometryType.esriGeometryPolygon:
                                        pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                                        break;
                                    default:
                                        break;
                                }
                                pSpatialFilter.GeometryField = pFeatureClass.ShapeFieldName;
                                pFilter = pSpatialFilter;
                                //Do the Search 从图层中查询出满足约束条件的元素
                                IFeatureCursor pCursor = trainstationlayer.Search(pFilter, false);
                                //select
                                IFeature ifeature = pCursor.NextFeature();
                                if (ifeature == null) throw new Exception();
                                while (ifeature != null)
                                {
                                    imap.SelectFeature(pFeatureLayer, ifeature);
                                    ifeature = pCursor.NextFeature();
                                }
                                axMapControl1.AddLayer(trainstationlayer);
                                this.trainstationbutton.Checked = true;
                                imap.SelectFeature(trainstationlayer, ifeature);
                            }
                            catch (Exception)
                            {
                                errorflag++;
                                try
                                {
                                    IFeatureLayer pFeatureLayer = supermarketlayer as IFeatureLayer;
                                    IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                                    switch (pFeatureClass.ShapeType)
                                    {
                                        case esriGeometryType.esriGeometryPoint:
                                            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                                            break;
                                        case esriGeometryType.esriGeometryPolyline:
                                            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
                                            break;
                                        case esriGeometryType.esriGeometryPolygon:
                                            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                                            break;
                                        default:
                                            break;
                                    }
                                    pSpatialFilter.GeometryField = pFeatureClass.ShapeFieldName;
                                    pFilter = pSpatialFilter;
                                    //Do the Search 从图层中查询出满足约束条件的元素
                                    IFeatureCursor pCursor = supermarketlayer.Search(pFilter, false);
                                    //select
                                    IFeature ifeature = pCursor.NextFeature();
                                    if (ifeature == null) throw new Exception();
                                    while (ifeature != null)
                                    {
                                        imap.SelectFeature(pFeatureLayer, ifeature);
                                        ifeature = pCursor.NextFeature();
                                    }
                                    axMapControl1.AddLayer(supermarketlayer);
                                    this.supermarketbutton.Checked = true;
                                    imap.SelectFeature(supermarketlayer, ifeature);
                                }
                                catch (Exception)
                                {
                                    errorflag++;
                                    try
                                    {
                                        IFeatureLayer pFeatureLayer = banklayer as IFeatureLayer;
                                        IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                                        switch (pFeatureClass.ShapeType)
                                        {
                                            case esriGeometryType.esriGeometryPoint:
                                                pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                                                break;
                                            case esriGeometryType.esriGeometryPolyline:
                                                pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
                                                break;
                                            case esriGeometryType.esriGeometryPolygon:
                                                pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                                                break;
                                            default:
                                                break;
                                        }
                                        pSpatialFilter.GeometryField = pFeatureClass.ShapeFieldName;
                                        pFilter = pSpatialFilter;
                                        //Do the Search 从图层中查询出满足约束条件的元素
                                        IFeatureCursor pCursor = banklayer.Search(pFilter, false);
                                        //select
                                        IFeature ifeature = pCursor.NextFeature();
                                        if (ifeature == null) throw new Exception();
                                        while (ifeature != null)
                                        {
                                            imap.SelectFeature(pFeatureLayer, ifeature);
                                            ifeature = pCursor.NextFeature();
                                        }
                                        axMapControl1.AddLayer(banklayer);
                                        this.bankbutton.Checked = true;
                                        imap.SelectFeature(banklayer, ifeature);
                                    }
                                    catch (Exception)
                                    {
                                        errorflag++;
                                        try
                                        {
                                            IFeatureLayer pFeatureLayer = governmentlayer as IFeatureLayer;
                                            IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                                            switch (pFeatureClass.ShapeType)
                                            {
                                                case esriGeometryType.esriGeometryPoint:
                                                    pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                                                    break;
                                                case esriGeometryType.esriGeometryPolyline:
                                                    pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
                                                    break;
                                                case esriGeometryType.esriGeometryPolygon:
                                                    pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                                                    break;
                                                default:
                                                    break;
                                            }
                                            pSpatialFilter.GeometryField = pFeatureClass.ShapeFieldName;
                                            pFilter = pSpatialFilter;
                                            //Do the Search 从图层中查询出满足约束条件的元素
                                            IFeatureCursor pCursor = governmentlayer.Search(pFilter, false);
                                            //select
                                            IFeature ifeature = pCursor.NextFeature();
                                            if (ifeature == null) throw new Exception();
                                            while (ifeature != null)
                                            {
                                                imap.SelectFeature(pFeatureLayer, ifeature);
                                                ifeature = pCursor.NextFeature();
                                            }
                                            axMapControl1.AddLayer(governmentlayer);
                                            this.governmentbutton.Checked = true;
                                            imap.SelectFeature(governmentlayer, ifeature);
                                        }
                                        catch (Exception)
                                        {
                                            errorflag++;
                                            try
                                            {
                                                IFeatureLayer pFeatureLayer = hospitallayer as IFeatureLayer;
                                                IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                                                switch (pFeatureClass.ShapeType)
                                                {
                                                    case esriGeometryType.esriGeometryPoint:
                                                        pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                                                        break;
                                                    case esriGeometryType.esriGeometryPolyline:
                                                        pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
                                                        break;
                                                    case esriGeometryType.esriGeometryPolygon:
                                                        pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                                                        break;
                                                    default:
                                                        break;
                                                }
                                                pSpatialFilter.GeometryField = pFeatureClass.ShapeFieldName;
                                                pFilter = pSpatialFilter;
                                                //Do the Search 从图层中查询出满足约束条件的元素
                                                IFeatureCursor pCursor = hospitallayer.Search(pFilter, false);
                                                //select
                                                IFeature ifeature = pCursor.NextFeature();
                                                if (ifeature == null) throw new Exception();
                                                while (ifeature != null)
                                                {
                                                    imap.SelectFeature(pFeatureLayer, ifeature);
                                                    ifeature = pCursor.NextFeature();
                                                }
                                                axMapControl1.AddLayer(hospitallayer);
                                                this.hospitalbutton.Checked = true;
                                                imap.SelectFeature(hospitallayer, ifeature);
                                            }
                                            catch (Exception)
                                            {
                                                errorflag++;
                                                try
                                                {
                                                    IFeatureLayer pFeatureLayer = railwaylayer as IFeatureLayer;
                                                    IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                                                    switch (pFeatureClass.ShapeType)
                                                    {
                                                        case esriGeometryType.esriGeometryPoint:
                                                            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                                                            break;
                                                        case esriGeometryType.esriGeometryPolyline:
                                                            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
                                                            break;
                                                        case esriGeometryType.esriGeometryPolygon:
                                                            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                                                            break;
                                                        default:
                                                            break;
                                                    }
                                                    pSpatialFilter.GeometryField = pFeatureClass.ShapeFieldName;
                                                    pFilter = pSpatialFilter;
                                                    //Do the Search 从图层中查询出满足约束条件的元素
                                                    IFeatureCursor pCursor = railwaylayer.Search(pFilter, false);
                                                    //select
                                                    IFeature ifeature = pCursor.NextFeature();
                                                    if (ifeature == null) throw new Exception();
                                                    while (ifeature != null)
                                                    {
                                                        imap.SelectFeature(pFeatureLayer, ifeature);
                                                        ifeature = pCursor.NextFeature();
                                                    }
                                                    axMapControl1.AddLayer(railwaylayer);
                                                    this.railwaybutton.Checked = true;
                                                    imap.SelectFeature(railwaylayer, ifeature);
                                                }
                                                catch (Exception)
                                                {
                                                    errorflag++;
                                                    try
                                                    {
                                                        IFeatureLayer pFeatureLayer = recreationlayer as IFeatureLayer;
                                                        IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                                                        switch (pFeatureClass.ShapeType)
                                                        {
                                                            case esriGeometryType.esriGeometryPoint:
                                                                pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                                                                break;
                                                            case esriGeometryType.esriGeometryPolyline:
                                                                pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
                                                                break;
                                                            case esriGeometryType.esriGeometryPolygon:
                                                                pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                                                                break;
                                                            default:
                                                                break;
                                                        }
                                                        pSpatialFilter.GeometryField = pFeatureClass.ShapeFieldName;
                                                        pFilter = pSpatialFilter;
                                                        //Do the Search 从图层中查询出满足约束条件的元素
                                                        IFeatureCursor pCursor = recreationlayer.Search(pFilter, false);
                                                        //select
                                                        IFeature ifeature = pCursor.NextFeature();
                                                        if (ifeature == null) throw new Exception();
                                                        while (ifeature != null)
                                                        {
                                                            imap.SelectFeature(pFeatureLayer, ifeature);
                                                            ifeature = pCursor.NextFeature();
                                                        }
                                                        axMapControl1.AddLayer(recreationlayer);
                                                        this.recreationbutton.Checked = true;
                                                        imap.SelectFeature(recreationlayer, ifeature);
                                                    }
                                                    catch (Exception)
                                                    {
                                                        errorflag++;
                                                        try
                                                        {
                                                            IFeatureLayer pFeatureLayer = hotellayer as IFeatureLayer;
                                                            IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                                                            switch (pFeatureClass.ShapeType)
                                                            {
                                                                case esriGeometryType.esriGeometryPoint:
                                                                    pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                                                                    break;
                                                                case esriGeometryType.esriGeometryPolyline:
                                                                    pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
                                                                    break;
                                                                case esriGeometryType.esriGeometryPolygon:
                                                                    pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                            pSpatialFilter.GeometryField = pFeatureClass.ShapeFieldName;
                                                            pFilter = pSpatialFilter;
                                                            //Do the Search 从图层中查询出满足约束条件的元素
                                                            IFeatureCursor pCursor = hotellayer.Search(pFilter, false);
                                                            //select
                                                            IFeature ifeature = pCursor.NextFeature();
                                                            if (ifeature == null) throw new Exception();
                                                            while (ifeature != null)
                                                            {
                                                                imap.SelectFeature(pFeatureLayer, ifeature);
                                                                ifeature = pCursor.NextFeature();
                                                            }
                                                            axMapControl1.AddLayer(hotellayer);
                                                            this.hotelbutton.Checked = true;
                                                            imap.SelectFeature(hotellayer, ifeature);
                                                        }
                                                        catch (Exception)
                                                        {
                                                            errorflag++;
                                                            try
                                                            {
                                                                IFeatureLayer pFeatureLayer = sightlayer as IFeatureLayer;
                                                                IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                                                                switch (pFeatureClass.ShapeType)
                                                                {
                                                                    case esriGeometryType.esriGeometryPoint:
                                                                        pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                                                                        break;
                                                                    case esriGeometryType.esriGeometryPolyline:
                                                                        pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
                                                                        break;
                                                                    case esriGeometryType.esriGeometryPolygon:
                                                                        pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                                                                        break;
                                                                    default:
                                                                        break;
                                                                }
                                                                pSpatialFilter.GeometryField = pFeatureClass.ShapeFieldName;
                                                                pFilter = pSpatialFilter;
                                                                //Do the Search 从图层中查询出满足约束条件的元素
                                                                IFeatureCursor pCursor = sightlayer.Search(pFilter, false);
                                                                //select
                                                                IFeature ifeature = pCursor.NextFeature();
                                                                if (ifeature == null) throw new Exception();
                                                                while (ifeature != null)
                                                                {
                                                                    imap.SelectFeature(pFeatureLayer, ifeature);
                                                                    ifeature = pCursor.NextFeature();
                                                                }
                                                                axMapControl1.AddLayer(sightlayer);
                                                                this.sightbutton.Checked = true;
                                                                imap.SelectFeature(sightlayer, ifeature);
                                                            }
                                                            catch (Exception)
                                                            {
                                                                errorflag++;
                                                                try
                                                                {
                                                                    IFeatureLayer pFeatureLayer = busstationlayer as IFeatureLayer;
                                                                    IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                                                                    switch (pFeatureClass.ShapeType)
                                                                    {
                                                                        case esriGeometryType.esriGeometryPoint:
                                                                            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                                                                            break;
                                                                        case esriGeometryType.esriGeometryPolyline:
                                                                            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
                                                                            break;
                                                                        case esriGeometryType.esriGeometryPolygon:
                                                                            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                                                                            break;
                                                                        default:
                                                                            break;
                                                                    }
                                                                    pSpatialFilter.GeometryField = pFeatureClass.ShapeFieldName;
                                                                    pFilter = pSpatialFilter;
                                                                    //Do the Search 从图层中查询出满足约束条件的元素
                                                                    IFeatureCursor pCursor = busstationlayer.Search(pFilter, false);
                                                                    //select
                                                                    IFeature ifeature = pCursor.NextFeature();
                                                                    if (ifeature == null) throw new Exception();
                                                                    while (ifeature != null)
                                                                    {
                                                                        imap.SelectFeature(pFeatureLayer, ifeature);
                                                                        ifeature = pCursor.NextFeature();
                                                                    }
                                                                    axMapControl1.AddLayer(busstationlayer);
                                                                    this.busstationbutton.Checked = true;
                                                                    imap.SelectFeature(busstationlayer, ifeature);
                                                                }
                                                                catch (Exception)
                                                                {
                                                                    errorflag++;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                axMapControl1.ActiveView.Refresh();
                if (errorflag == 14)
                    MessageBox.Show("没有选取到点！");
            }
        }

        private double ConvertPixelsToMapUnits(IActiveView pActiveView, double pixelUnits)
        {
            // Uses the ratio of the size of the map in pixels to map units to do the conversion
            IPoint p1 = pActiveView.ScreenDisplay.DisplayTransformation.VisibleBounds.UpperLeft;
            IPoint p2 = pActiveView.ScreenDisplay.DisplayTransformation.VisibleBounds.UpperRight;
            int x1, x2, y1, y2;
            pActiveView.ScreenDisplay.DisplayTransformation.FromMapPoint(p1, out x1, out y1);
            pActiveView.ScreenDisplay.DisplayTransformation.FromMapPoint(p2, out x2, out y2);
            double pixelExtent = x2 - x1;
            double realWorldDisplayExtent = pActiveView.ScreenDisplay.DisplayTransformation.VisibleBounds.Width;
            double sizeOfOnePixel = realWorldDisplayExtent / pixelExtent;
            return pixelUnits * sizeOfOnePixel;
        }

        private void searchitem_Click(object sender, EventArgs e)
        {
            int error = 0;
            IQueryFilter pqueryfilter = new QueryFilterClass();
            pqueryfilter.WhereClause = "名称= '" + this.searchtextbox.Text.Trim() + "'";
            IMap imap = axMapControl1.Map;
            try
            {
                IFeatureSelection trainstationLayerSelection = (IFeatureSelection)trainstationlayer;
                trainstationLayerSelection.SelectFeatures(pqueryfilter, esriSelectionResultEnum.esriSelectionResultNew, true);
                IFeature ifeature1 = trainstation.GetFeature(trainstationLayerSelection.SelectionSet.IDs.Next());
                axMapControl1.AddLayer(trainstationlayer);
                this.trainstationbutton.Checked = true;
                axMapControl1.FlashShape(ifeature1.Shape);
                imap.SelectFeature(trainstationlayer, ifeature1);
            }
            catch (Exception)
            {
                error++;
                try
                {
                    IFeatureSelection supermarketLayerSelection = (IFeatureSelection)supermarketlayer;
                    supermarketLayerSelection.SelectFeatures(pqueryfilter, esriSelectionResultEnum.esriSelectionResultNew, true);
                    IFeature ifeature2 = supermarket.GetFeature(supermarketLayerSelection.SelectionSet.IDs.Next());
                    axMapControl1.AddLayer(supermarketlayer);
                    this.supermarketbutton.Checked = true;
                    axMapControl1.FlashShape(ifeature2.Shape);
                    imap.SelectFeature(supermarketlayer, ifeature2);
                }
                catch (Exception)
                {
                    error++;
                    try
                    {
                        IFeatureSelection addressLayerSelection = (IFeatureSelection)addresslayer;
                        addressLayerSelection.SelectFeatures(pqueryfilter, esriSelectionResultEnum.esriSelectionResultNew, true);
                        IFeature ifeature3 = address.GetFeature(addressLayerSelection.SelectionSet.IDs.Next());
                        axMapControl1.AddLayer(addresslayer);
                        this.addressbutton.Checked = true;
                        axMapControl1.FlashShape(ifeature3.Shape);
                        imap.SelectFeature(addresslayer, ifeature3);
                    }
                    catch (Exception)
                    {
                        error++;
                        try
                        {
                            IFeatureSelection hotelLayerSelection = (IFeatureSelection)hotellayer;
                            hotelLayerSelection.SelectFeatures(pqueryfilter, esriSelectionResultEnum.esriSelectionResultNew, true);
                            IFeature ifeature4 = hotel.GetFeature(hotelLayerSelection.SelectionSet.IDs.Next());
                            axMapControl1.AddLayer(hotellayer);
                            this.hotelbutton.Checked = true;
                            axMapControl1.FlashShape(ifeature4.Shape);
                            imap.SelectFeature(hotellayer, ifeature4);
                        }
                        catch (Exception)
                        {
                            error++;
                            try
                            {
                                IFeatureSelection bankLayerSelection = (IFeatureSelection)banklayer;
                                bankLayerSelection.SelectFeatures(pqueryfilter, esriSelectionResultEnum.esriSelectionResultNew, true);
                                IFeature ifeature5 = bank.GetFeature(bankLayerSelection.SelectionSet.IDs.Next());
                                axMapControl1.AddLayer(banklayer);
                                this.bankbutton.Checked = true;
                                axMapControl1.FlashShape(ifeature5.Shape);
                                imap.SelectFeature(banklayer, ifeature5);
                            }
                            catch (Exception)
                            {
                                error++;
                                try
                                {
                                    IFeatureSelection sightLayerSelection = (IFeatureSelection)sightlayer;
                                    sightLayerSelection.SelectFeatures(pqueryfilter, esriSelectionResultEnum.esriSelectionResultNew, true);
                                    IFeature ifeature6 = sight.GetFeature(sightLayerSelection.SelectionSet.IDs.Next());
                                    axMapControl1.AddLayer(sightlayer);
                                    this.sightbutton.Checked = true;
                                    axMapControl1.FlashShape(ifeature6.Shape);
                                    imap.SelectFeature(sightlayer, ifeature6);
                                }
                                catch (Exception)
                                {
                                    error++;
                                    try
                                    {
                                        IFeatureSelection busstationLayerSelection = (IFeatureSelection)busstationlayer;
                                        busstationLayerSelection.SelectFeatures(pqueryfilter, esriSelectionResultEnum.esriSelectionResultNew, true);
                                        IFeature ifeature7 = busstation.GetFeature(busstationLayerSelection.SelectionSet.IDs.Next());
                                        axMapControl1.AddLayer(busstationlayer);
                                        this.busstationbutton.Checked = true;
                                        axMapControl1.FlashShape(ifeature7.Shape);
                                        imap.SelectFeature(busstationlayer, ifeature7);
                                    }
                                    catch (Exception)
                                    {
                                        error++;
                                        try
                                        {
                                            IFeatureSelection governmentLayerSelection = (IFeatureSelection)governmentlayer;
                                            governmentLayerSelection.SelectFeatures(pqueryfilter, esriSelectionResultEnum.esriSelectionResultNew, true);
                                            IFeature ifeature8 = government.GetFeature(governmentLayerSelection.SelectionSet.IDs.Next());
                                            axMapControl1.AddLayer(governmentlayer);
                                            this.governmentbutton.Checked = true;
                                            axMapControl1.FlashShape(ifeature8.Shape);
                                            imap.SelectFeature(governmentlayer, ifeature8);
                                        }
                                        catch (Exception)
                                        {
                                            error++;
                                            try
                                            {
                                                IFeatureSelection hospitalLayerSelection = (IFeatureSelection)hospitallayer;
                                                hospitalLayerSelection.SelectFeatures(pqueryfilter, esriSelectionResultEnum.esriSelectionResultNew, true);
                                                IFeature ifeature9 = hospital.GetFeature(hospitalLayerSelection.SelectionSet.IDs.Next());
                                                axMapControl1.AddLayer(hospitallayer);
                                                this.hospitalbutton.Checked = true;
                                                axMapControl1.FlashShape(ifeature9.Shape);
                                                imap.SelectFeature(hospitallayer, ifeature9);
                                            }
                                            catch (Exception)
                                            {
                                                error++;
                                                try
                                                {
                                                    IFeatureSelection recreationLayerSelection = (IFeatureSelection)recreationlayer;
                                                    recreationLayerSelection.SelectFeatures(pqueryfilter, esriSelectionResultEnum.esriSelectionResultNew, true);
                                                    IFeature ifeature10 = recreation.GetFeature(recreationLayerSelection.SelectionSet.IDs.Next());
                                                    axMapControl1.AddLayer(recreationlayer);
                                                    this.recreationbutton.Checked = true;
                                                    axMapControl1.FlashShape(ifeature10.Shape);
                                                    imap.SelectFeature(recreationlayer, ifeature10);
                                                }
                                                catch (Exception) { error++; }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                if (error == 10)
                {
                    MessageBox.Show("没有找到该地物！");
                    this.zoomtofeature.Enabled = false;
                }
                else this.zoomtofeature.Enabled = true;
            }
        }
        #endregion

        #region layerbutton
        private void addressbutton_Click(object sender, EventArgs e)
        {
            if (addressbutton.Checked)
            {
                axMapControl1.AddLayer(addresslayer);
            }
            else
            {
                IMapLayers mapLayers = axMapControl1.Map as IMapLayers;
                mapLayers.DeleteLayer(addresslayer);
            }
        }

        private void supermarketbutton_Click(object sender, EventArgs e)
        {
            if (supermarketbutton.Checked)
            {
                axMapControl1.AddLayer(supermarketlayer);
            }
            else
            {
                IMapLayers mapLayers = axMapControl1.Map as IMapLayers;
                mapLayers.DeleteLayer(supermarketlayer);
            }
        }

        private void bankbutton_Click(object sender, EventArgs e)
        {
            if (bankbutton.Checked)
            {
                axMapControl1.AddLayer(banklayer);
            }
            else
            {
                IMapLayers mapLayers = axMapControl1.Map as IMapLayers;
                mapLayers.DeleteLayer(banklayer);
            }
        }

        private void governmentbutton_Click(object sender, EventArgs e)
        {
            if (governmentbutton.Checked)
            {
                axMapControl1.AddLayer(governmentlayer);
            }
            else
            {
                IMapLayers mapLayers = axMapControl1.Map as IMapLayers;
                mapLayers.DeleteLayer(governmentlayer);
            }
        }

        private void hospitalbutton_Click(object sender, EventArgs e)
        {
            if (hospitalbutton.Checked)
            {
                axMapControl1.AddLayer(hospitallayer);
            }
            else
            {
                IMapLayers mapLayers = axMapControl1.Map as IMapLayers;
                mapLayers.DeleteLayer(hospitallayer);
            }
        }

        private void secondroadbutton_Click(object sender, EventArgs e)
        {
            if (secondroadbutton.Checked)
            {
                axMapControl1.AddLayer(secondroadlayer);
            }
            else
            {
                IMapLayers mapLayers = axMapControl1.Map as IMapLayers;
                mapLayers.DeleteLayer(secondroadlayer);
            }
        }

        private void waterregionbutton_Click(object sender, EventArgs e)
        {
            if (waterregionbutton.Checked)
            {
                axMapControl1.AddLayer(waterregionlayer);
            }
            else
            {
                IMapLayers mapLayers = axMapControl1.Map as IMapLayers;
                mapLayers.DeleteLayer(waterregionlayer);
            }
        }

        private void railwaybutton_Click(object sender, EventArgs e)
        {
            if (railwaybutton.Checked)
            {
                axMapControl1.AddLayer(railwaylayer);
            }
            else
            {
                IMapLayers mapLayers = axMapControl1.Map as IMapLayers;
                mapLayers.DeleteLayer(railwaylayer);
            }
        }

        private void recreationbutton_Click(object sender, EventArgs e)
        {
            if (recreationbutton.Checked)
            {
                axMapControl1.AddLayer(recreationlayer);
            }
            else
            {
                IMapLayers mapLayers = axMapControl1.Map as IMapLayers;
                mapLayers.DeleteLayer(recreationlayer);
            }
        }

        private void hotelbutton_Click(object sender, EventArgs e)
        {
            if (hotelbutton.Checked)
            {
                axMapControl1.AddLayer(hotellayer);
            }
            else
            {
                IMapLayers mapLayers = axMapControl1.Map as IMapLayers;
                mapLayers.DeleteLayer(hotellayer);
            }
        }

        private void sightbutton_Click(object sender, EventArgs e)
        {
            if (sightbutton.Checked)
            {
                axMapControl1.AddLayer(sightlayer);
            }
            else
            {
                IMapLayers mapLayers = axMapControl1.Map as IMapLayers;
                mapLayers.DeleteLayer(sightlayer);
            }
        }

        private void mainroadbutton_Click(object sender, EventArgs e)
        {
            if (mainroadbutton.Checked)
            {
                axMapControl1.AddLayer(mainroadlayer);
            }
            else
            {
                IMapLayers mapLayers = axMapControl1.Map as IMapLayers;
                mapLayers.DeleteLayer(mainroadlayer);
            }
        }

        private void busstationbutton_Click(object sender, EventArgs e)
        {
            if (busstationbutton.Checked)
            {
                axMapControl1.AddLayer(busstationlayer);
            }
            else
            {
                IMapLayers mapLayers = axMapControl1.Map as IMapLayers;
                mapLayers.DeleteLayer(busstationlayer);
            }
        }

        private void trainstationbutton_Click(object sender, EventArgs e)
        {
            if (trainstationbutton.Checked)
            {
                axMapControl1.AddLayer(trainstationlayer);
            }
            else
            {
                IMapLayers mapLayers = axMapControl1.Map as IMapLayers;
                mapLayers.DeleteLayer(trainstationlayer);
            }
        }
        #endregion

        private void aboutus_Click(object sender, EventArgs e)
        {
            MessageBox.Show("胡添毅 柳登科 王旭灿 熊曼迪\r\n2014年1月16日");
        }

        private void rainmode_Click(object sender, EventArgs e)
        {
            if (modeflag == 1)
            {
                rainmode.Text = "切换为晴天模式";
                modeflag = 2;
                this.riverlevelbar.Visible = true;
                barrierarray1.Clear();
                barrierarray2.Clear();
                clsload();
                zoomin.Checked = false;
                zoomout.Checked = false;
                pan.Checked = false;
                findpath.Checked = false;
                startcalculating.Checked = false;
                select.Checked = false;
                distanceprocess.Checked = false;
                IGraphicsContainer pgraghicscontainer = axMapControl1.Map as IGraphicsContainer;
                IActiveView pactiveview = pgraghicscontainer as IActiveView;
                axMapControl1.Extent = pactiveview.FullExtent;
                axMapControl1.ActiveView.Refresh();
                axMapControl1.CurrentTool = null;
            }
            else
            {
                rainmode.Text = "切换为雨天模式";
                modeflag = 1;
                this.riverlevelbar.Visible = false;
                zoomin.Checked = false;
                zoomout.Checked = false;
                pan.Checked = false;
                findpath.Checked = false;
                startcalculating.Checked = false;
                select.Checked = false;
                distanceprocess.Checked = false;
                IGraphicsContainer pgraghicscontainer = axMapControl1.Map as IGraphicsContainer;
                IActiveView pactiveview = pgraghicscontainer as IActiveView;
                axMapControl1.Extent = pactiveview.FullExtent;
                axMapControl1.ActiveView.Refresh();
                axMapControl1.CurrentTool = null;
            }
        }

        private void riverlevel_Click(object sender, EventArgs e)
        {
            rainmode2();
        }

        private void cleardrain_Click(object sender, EventArgs e)
        {
            zoomin.Checked = false;
            zoomout.Checked = false;
            pan.Checked = false;
            findpath.Checked = false;
            startcalculating.Checked = false;
            select.Checked = false;
            distanceprocess.Checked = false;
            IGraphicsContainer pgraghicscontainer = axMapControl1.Map as IGraphicsContainer;
            IActiveView pactiveview = pgraghicscontainer as IActiveView;
            axMapControl1.Extent = pactiveview.FullExtent;
            axMapControl1.CurrentTool = null;
        }

        private void rainmode2()
        {
            try
            {
                barrierarray1.Clear();
                barrierarray2.Clear();
                string text = this.riverleveltextbox.Text;
                double d = Convert.ToDouble(text);
                if (d >= 30)
                {
                    MessageBox.Show("全城渍水，宅在家里吧！");
                }
                #region 20~25
                if (d > 20 && d < 26)
                {
                    Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
                    IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
                    IWorkspace workspace = workspaceFactory.OpenFromFile(@"C:\Users\huty\Desktop\武汉实习数据\道路.gdb", 0);
                    IFeatureWorkspace featureworkspace = (IFeatureWorkspace)workspace;
                    IEnumDataset pEnumDataset = workspace.get_Datasets(ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTAny);
                    IMap ipmap = this.axMapControl1.ActiveView.FocusMap;
                    cls.SetOrGetMap = ipmap;
                    IDataset pDataset = pEnumDataset.Next();
                    IFeatureDataset Featuredataset = featureworkspace.OpenFeatureDataset(pDataset.Name);

                    IFeatureClass ifeatureclass1 = featureworkspace.OpenFeatureClass("class1");
                    IQueryFilter queryFilter1 = new QueryFilterClass();
                    queryFilter1.WhereClause = "可承载水位 = 23";
                    IFeatureCursor featurecursor1 = ifeatureclass1.Search(queryFilter1, false);
                    IFeature ifeature1 = featurecursor1.NextFeature();
                    while (ifeature1 != null)
                    {
                        int i = ifeature1.OID;
                        barrierarray1.Add(i);
                        ifeature1 = featurecursor1.NextFeature();
                        IPolyline ipolyline;
                        if (ifeature1 != null)
                        {
                            ipolyline = ifeature1.Shape as IPolyline;
                            drawpathline2(ipolyline);
                        }
                    }

                    IFeatureClass ifeatureclass2 = featureworkspace.OpenFeatureClass("class2");
                    IQueryFilter queryFilter2 = new QueryFilterClass();
                    queryFilter2.WhereClause = "可承载水位 = 23";
                    IFeatureCursor featurecursor2 = ifeatureclass2.Search(queryFilter2, false);
                    IFeature ifeature2 = featurecursor2.NextFeature();
                    while (ifeature2 != null)
                    {
                        int i = ifeature2.OID;
                        barrierarray2.Add(i);
                        ifeature2 = featurecursor2.NextFeature();
                        IPolyline ipolyline;
                        if (ifeature2 != null)
                        {
                            ipolyline = ifeature2.Shape as IPolyline;
                            drawpathline2(ipolyline);
                        }
                    }
                    cls.OpenFeatureDatasetNetwork(Featuredataset, barrierarray1, barrierarray2, 1);
                }
                #endregion
                #region 26~27
                if (d >= 26 && d < 28)
                {
                    Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
                    IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
                    IWorkspace workspace = workspaceFactory.OpenFromFile(@"C:\Users\huty\Desktop\武汉实习数据\道路.gdb", 0);
                    IFeatureWorkspace featureworkspace = (IFeatureWorkspace)workspace;
                    IEnumDataset pEnumDataset = workspace.get_Datasets(ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTAny);
                    IMap ipmap = this.axMapControl1.ActiveView.FocusMap;
                    cls.SetOrGetMap = ipmap;
                    IDataset pDataset = pEnumDataset.Next();
                    IFeatureDataset Featuredataset = featureworkspace.OpenFeatureDataset(pDataset.Name);

                    IFeatureClass ifeatureclass1 = featureworkspace.OpenFeatureClass("class1");
                    IQueryFilter queryFilter1 = new QueryFilterClass();
                    queryFilter1.WhereClause = "可承载水位 = 23";
                    IFeatureCursor featurecursor1 = ifeatureclass1.Search(queryFilter1, false);
                    IFeature ifeature1 = featurecursor1.NextFeature();
                    while (ifeature1 != null)
                    {
                        int i = ifeature1.OID;
                        barrierarray1.Add(i);
                        ifeature1 = featurecursor1.NextFeature();
                        IPolyline ipolyline;
                        if (ifeature1 != null)
                        {
                            ipolyline = ifeature1.Shape as IPolyline;
                            drawpathline2(ipolyline);
                        }
                    }

                    IFeatureClass ifeatureclass2 = featureworkspace.OpenFeatureClass("class2");
                    IQueryFilter queryFilter2 = new QueryFilterClass();
                    queryFilter2.WhereClause = "可承载水位 = 23";
                    IFeatureCursor featurecursor2 = ifeatureclass2.Search(queryFilter2, false);
                    IFeature ifeature2 = featurecursor2.NextFeature();
                    while (ifeature2 != null)
                    {
                        int i = ifeature2.OID;
                        barrierarray2.Add(i);
                        ifeature2 = featurecursor2.NextFeature();
                        IPolyline ipolyline;
                        if (ifeature2 != null)
                        {
                            ipolyline = ifeature2.Shape as IPolyline;
                            drawpathline2(ipolyline);
                        }
                    }

                    IFeatureClass ifeatureclass3 = featureworkspace.OpenFeatureClass("class1");
                    IQueryFilter queryFilter3 = new QueryFilterClass();
                    queryFilter3.WhereClause = "可承载水位 = 25";
                    IFeatureCursor featurecursor3 = ifeatureclass3.Search(queryFilter3, false);
                    IFeature ifeature3 = featurecursor3.NextFeature();
                    while (ifeature3 != null)
                    {
                        int i = ifeature3.OID;
                        barrierarray1.Add(i);
                        ifeature3 = featurecursor3.NextFeature();
                        IPolyline ipolyline;
                        if (ifeature3 != null)
                        {
                            ipolyline = ifeature3.Shape as IPolyline;
                            drawpathline2(ipolyline);
                        }
                    }

                    IFeatureClass ifeatureclass4 = featureworkspace.OpenFeatureClass("class2");
                    IQueryFilter queryFilter4 = new QueryFilterClass();
                    queryFilter4.WhereClause = "可承载水位 = 23";
                    IFeatureCursor featurecursor4 = ifeatureclass4.Search(queryFilter4, false);
                    IFeature ifeature4 = featurecursor4.NextFeature();
                    while (ifeature4 != null)
                    {
                        int i = ifeature4.OID;
                        barrierarray2.Add(i);
                        ifeature4 = featurecursor4.NextFeature();
                        IPolyline ipolyline;
                        if (ifeature4 != null)
                        {
                            ipolyline = ifeature4.Shape as IPolyline;
                            drawpathline2(ipolyline);
                        }
                    }
                    cls.OpenFeatureDatasetNetwork(Featuredataset, barrierarray1, barrierarray2, 1);
                }
                #endregion
                #region 28~29
                if (d >= 28 && d < 30)
                {
                    Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
                    IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
                    IWorkspace workspace = workspaceFactory.OpenFromFile(@"C:\Users\huty\Desktop\武汉实习数据\道路.gdb", 0);
                    IFeatureWorkspace featureworkspace = (IFeatureWorkspace)workspace;
                    IEnumDataset pEnumDataset = workspace.get_Datasets(ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTAny);
                    IMap ipmap = this.axMapControl1.ActiveView.FocusMap;
                    cls.SetOrGetMap = ipmap;
                    IDataset pDataset = pEnumDataset.Next();
                    IFeatureDataset Featuredataset = featureworkspace.OpenFeatureDataset(pDataset.Name);

                    IFeatureClass ifeatureclass1 = featureworkspace.OpenFeatureClass("class1");
                    IQueryFilter queryFilter1 = new QueryFilterClass();
                    queryFilter1.WhereClause = "可承载水位 = 23";
                    IFeatureCursor featurecursor1 = ifeatureclass1.Search(queryFilter1, false);
                    IFeature ifeature1 = featurecursor1.NextFeature();
                    while (ifeature1 != null)
                    {
                        int i = ifeature1.OID;
                        barrierarray1.Add(i);
                        ifeature1 = featurecursor1.NextFeature();
                        IPolyline ipolyline;
                        if (ifeature1 != null)
                        {
                            ipolyline = ifeature1.Shape as IPolyline;
                            drawpathline2(ipolyline);
                        }
                    }

                    IFeatureClass ifeatureclass2 = featureworkspace.OpenFeatureClass("class2");
                    IQueryFilter queryFilter2 = new QueryFilterClass();
                    queryFilter2.WhereClause = "可承载水位 = 23";
                    IFeatureCursor featurecursor2 = ifeatureclass2.Search(queryFilter2, false);
                    IFeature ifeature2 = featurecursor2.NextFeature();
                    while (ifeature2 != null)
                    {
                        int i = ifeature2.OID;
                        barrierarray2.Add(i);
                        ifeature2 = featurecursor2.NextFeature();
                        IPolyline ipolyline;
                        if (ifeature2 != null)
                        {
                            ipolyline = ifeature2.Shape as IPolyline;
                            drawpathline2(ipolyline);
                        }
                    }

                    IFeatureClass ifeatureclass3 = featureworkspace.OpenFeatureClass("class1");
                    IQueryFilter queryFilter3 = new QueryFilterClass();
                    queryFilter3.WhereClause = "可承载水位 = 25";
                    IFeatureCursor featurecursor3 = ifeatureclass3.Search(queryFilter3, false);
                    IFeature ifeature3 = featurecursor3.NextFeature();
                    while (ifeature3 != null)
                    {
                        int i = ifeature3.OID;
                        barrierarray1.Add(i);
                        ifeature3 = featurecursor3.NextFeature();
                        IPolyline ipolyline;
                        if (ifeature3 != null)
                        {
                            ipolyline = ifeature3.Shape as IPolyline;
                            drawpathline2(ipolyline);
                        }
                    }

                    IFeatureClass ifeatureclass4 = featureworkspace.OpenFeatureClass("class2");
                    IQueryFilter queryFilter4 = new QueryFilterClass();
                    queryFilter4.WhereClause = "可承载水位 = 23";
                    IFeatureCursor featurecursor4 = ifeatureclass4.Search(queryFilter4, false);
                    IFeature ifeature4 = featurecursor4.NextFeature();
                    while (ifeature4 != null)
                    {
                        int i = ifeature4.OID;
                        barrierarray2.Add(i);
                        ifeature4 = featurecursor4.NextFeature();
                        IPolyline ipolyline;
                        if (ifeature4 != null)
                        {
                            ipolyline = ifeature4.Shape as IPolyline;
                            drawpathline2(ipolyline);
                        }
                    }

                    IFeatureClass ifeatureclass5 = featureworkspace.OpenFeatureClass("class1");
                    IQueryFilter queryFilter5 = new QueryFilterClass();
                    queryFilter5.WhereClause = "可承载水位 = 23";
                    IFeatureCursor featurecursor5 = ifeatureclass5.Search(queryFilter5, false);
                    IFeature ifeature5 = featurecursor5.NextFeature();
                    while (ifeature5 != null)
                    {
                        int i = ifeature5.OID;
                        barrierarray1.Add(i);
                        ifeature5 = featurecursor5.NextFeature();
                        IPolyline ipolyline;
                        if (ifeature5 != null)
                        {
                            ipolyline = ifeature5.Shape as IPolyline;
                            drawpathline2(ipolyline);
                        }
                    }

                    IFeatureClass ifeatureclass6 = featureworkspace.OpenFeatureClass("class2");
                    IQueryFilter queryFilter6 = new QueryFilterClass();
                    queryFilter6.WhereClause = "可承载水位 = 23";
                    IFeatureCursor featurecursor6 = ifeatureclass6.Search(queryFilter6, false);
                    IFeature ifeature6 = featurecursor6.NextFeature();
                    while (ifeature6 != null)
                    {
                        int i = ifeature6.OID;
                        barrierarray2.Add(i);
                        ifeature6 = featurecursor6.NextFeature();
                        IPolyline ipolyline;
                        if (ifeature6 != null)
                        {
                            ipolyline = ifeature6.Shape as IPolyline;
                            drawpathline2(ipolyline);
                        }
                    }
                    cls.OpenFeatureDatasetNetwork(Featuredataset, barrierarray1, barrierarray2, 1);
                }
                #endregion
            }
            catch (Exception)
            {
                MessageBox.Show("输入的格式有误！！");
            }
        }
    }
}
