using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.NetworkAnalysis;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.Geodatabase;

namespace trydotnet
{
    class ClsPathFinder
    {
        private IGeometricNetwork m_ipGeometricNetwork;
        private IMap m_ipMap;//输入地图
        private IPointCollection m_ipPoints;//输入经过节点的集合
        private IPointToEID m_ipPointToEID;//找到网格内距离点最近的元素
        private double m_dblPathCost = 0;//输出总权重
        private IEnumNetEID m_ipEnumNetEID_Junctions;
        private IEnumNetEID m_ipEnumNetEID_Edges;
        public IPolyline m_ipPolyline;//输出最短路径
        private System.Collections.ArrayList barrierarray1 = new System.Collections.ArrayList();
        private System.Collections.ArrayList barrierarray2 = new System.Collections.ArrayList();
        private static int barrierflag = 0;

        #region Public Function
        //返回和设置当前地图
        public IMap SetOrGetMap
        {
            set { m_ipMap = value; }
            get { return m_ipMap; }
        }

        //打开网络
        public void OpenFeatureDatasetNetwork(IFeatureDataset Featuredataset,System.Collections.ArrayList array1,System.Collections.ArrayList array2,int flag)
        {
            CloseWorkspace();
            if (!InitializeNetworkAndMap(Featuredataset,array1,array2,flag))
                Console.WriteLine("打开出错");
        }

        //输入点的集合
        public IPointCollection StopPoints
        {
            set { m_ipPoints = value; }
            get { return m_ipPoints; }
        }

        //路径成本
        public double PathCost
        {
            get { return m_dblPathCost; }
        }

        //返回路径
        public bool PathPolyLine()
        {
            try
            {
                IEIDInfo ipEIDInfo;
                IGeometry ipGeometry;
                if (m_ipPolyline != null) return true;
                m_ipPolyline = new PolylineClass();
                IGeometryCollection ipNewGeometryColl = m_ipPolyline as IGeometryCollection;
                ISpatialReference ipSpatialReference = m_ipMap.SpatialReference;
                IEIDHelper ipEIDHelper = new EIDHelperClass();
                ipEIDHelper.GeometricNetwork = m_ipGeometricNetwork;
                ipEIDHelper.OutputSpatialReference = ipSpatialReference;
                ipEIDHelper.ReturnGeometries = true;
                IEnumEIDInfo ipEnumEIDInfo = ipEIDHelper.CreateEnumEIDInfo(m_ipEnumNetEID_Edges);
                int count = ipEnumEIDInfo.Count;
                ipEnumEIDInfo.Reset();
                for (int i = 0; i < count; i++)
                {
                    ipEIDInfo = ipEnumEIDInfo.Next();
                    ipGeometry = ipEIDInfo.Geometry;
                    ipNewGeometryColl.AddGeometryCollection(ipGeometry as IGeometryCollection);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //解决路径
        public void SolvePath(string WeightName)
        {
            try
            {
                int intEdgeUserClassID;
                int intEdgeUserID;
                int intEdgeUserSubID;
                int intEdgeID;
                IPoint ipFoundEdgePoint;
                double dblEdgePercent;
                //定义追踪线
                ITraceFlowSolverGEN ipTraceFlowSolver = new TraceFlowSolverClass() as ITraceFlowSolverGEN;
                INetSolver ipNetSolver = ipTraceFlowSolver as INetSolver;
                //ipNetSolver.SelectionSetBarriers
                INetwork ipNetwork = m_ipGeometricNetwork.Network;
                ipNetSolver.SourceNetwork = ipNetwork;
                INetElements ipNetElements = ipNetwork as INetElements;
                int intCount = m_ipPoints.PointCount;
                //定义一个边线旗数组，各边线与输入点最近
                IEdgeFlag[] pEdgeFlagList = new EdgeFlagClass[intCount];
                for (int i = 0; i < intCount; i++)
                {
                    INetFlag ipNetFlag = new EdgeFlagClass() as INetFlag;
                    IPoint ipEdgePoint = m_ipPoints.get_Point(i);
                    //查找输入点的最近的边线
                    m_ipPointToEID.GetNearestEdge(ipEdgePoint, out intEdgeID, out ipFoundEdgePoint, out dblEdgePercent);
                    ipNetElements.QueryIDs(intEdgeID, esriElementType.esriETEdge, out intEdgeUserClassID, out intEdgeUserID, out intEdgeUserSubID);
                    ipNetFlag.UserClassID = intEdgeUserClassID;
                    ipNetFlag.UserID = intEdgeUserID;
                    ipNetFlag.UserSubID = intEdgeUserSubID;
                    IEdgeFlag pTemp = (IEdgeFlag)(ipNetFlag as IEdgeFlag);
                    pEdgeFlagList[i] = pTemp;
                }
                IFeatureClassContainer ipFeatureClassContainer = m_ipGeometricNetwork as IFeatureClassContainer;
                IFeatureClass ipFeatureClass;
                int count = ipFeatureClassContainer.ClassCount;
                if (barrierflag == 1)
                {
                    for (int i = 0; i < count; i++)
                    {
                        ipFeatureClass = ipFeatureClassContainer.get_Class(i);
                        INetElementBarriers inetelementbarriers;
                        if (i == 0)
                        {
                            inetelementbarriers = setbarriers(ipFeatureClass, barrierarray1, ipNetwork);
                            if(inetelementbarriers!=null)
                                ipNetSolver.set_ElementBarriers(esriElementType.esriETEdge, inetelementbarriers);
                        }
                        if (i == 1)
                        {
                            inetelementbarriers = setbarriers(ipFeatureClass, barrierarray2, ipNetwork);
                            if(inetelementbarriers!=null)
                                ipNetSolver.set_ElementBarriers(esriElementType.esriETEdge, inetelementbarriers);
                        }
                    }
                }
                ipTraceFlowSolver.PutEdgeOrigins(ref pEdgeFlagList);
                INetSchema ipNetSchema = ipNetwork as INetSchema;
                INetWeight ipNetWeight = ipNetSchema.get_WeightByName(WeightName);
                INetSolverWeights ipNetSolverWeights = ipTraceFlowSolver as INetSolverWeights;
                ipNetSolverWeights.FromToEdgeWeight = ipNetWeight;//开始边线的权重
                ipNetSolverWeights.ToFromEdgeWeight = ipNetWeight;//终止边线的权重
                object[] vaRes = new object[intCount - 1];
                //通过findpath得到边线和交汇点的集合
                ipTraceFlowSolver.FindPath(esriFlowMethod.esriFMConnected, esriShortestPathObjFn.esriSPObjFnMinSum, out m_ipEnumNetEID_Junctions, out m_ipEnumNetEID_Edges, intCount - 1, ref vaRes);
                //计算成本
                m_dblPathCost = 0;
                for (int i = 0; i < vaRes.Length; i++)
                {
                    double m_Va = (double)vaRes[i];
                    m_dblPathCost = m_dblPathCost + m_Va;
                }
                m_ipPolyline = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private INetElementBarriers setbarriers(IFeatureClass ifeatureclass, System.Collections.ArrayList barrierarray, INetwork ipNetwork)
        {
            INetElementBarriersGEN netEdgeBarriers = new NetElementBarriersClass() as INetElementBarriersGEN;
            int[] oidArray = new int[barrierarray.Count];
            if (barrierarray.Count > 0)
            {
                for (int i = 0; i < barrierarray.Count; i++)
                {
                    oidArray[i] = (int)barrierarray[i];
                }
                netEdgeBarriers.Network = ipNetwork;
                netEdgeBarriers.ElementType = esriElementType.esriETEdge;
                netEdgeBarriers.SetBarriers(ifeatureclass.FeatureClassID, oidArray);
                INetElementBarriers nb = netEdgeBarriers as INetElementBarriers;
                return nb;
            }
            else return null;
        }

        #endregion

        #region Private Function
        //初始化
        private bool InitializeNetworkAndMap(IFeatureDataset Featuredataset,System.Collections.ArrayList array1,System.Collections.ArrayList array2,int flag)
        {
            IFeatureClassContainer ipFeatureClassContainer;
            IFeatureClass ipFeatureClass;
            IGeoDataset ipGeoDataset;
            ILayer ipLayer;
            IFeatureLayer ipFeatureLayer;
            IEnvelope ipEnvelope, ipMaxEnvelope;
            double dblSearchTol;
            INetworkCollection ipNetworkCollection = Featuredataset as INetworkCollection;
            int count = ipNetworkCollection.GeometricNetworkCount;
            barrierarray1 =(System.Collections.ArrayList)array1.Clone();
            barrierarray2 = (System.Collections.ArrayList)array2.Clone();
            barrierflag = flag;
            //获取几何网络工作空间
            m_ipGeometricNetwork = ipNetworkCollection.get_GeometricNetwork(0);
            INetwork ipNetwork = m_ipGeometricNetwork.Network;
            if (m_ipMap != null)
            {
                m_ipMap = new MapClass();
                ipFeatureClassContainer = m_ipGeometricNetwork as IFeatureClassContainer;
                count = ipFeatureClassContainer.ClassCount;
                for (int i = 0; i < count; i++)
                {
                    ipFeatureClass = ipFeatureClassContainer.get_Class(i);
                    ipFeatureLayer = new FeatureLayerClass();
                    ipFeatureLayer.FeatureClass = ipFeatureClass;
                    m_ipMap.AddLayer(ipFeatureLayer);
                }
            }
            count = m_ipMap.LayerCount;
            ipMaxEnvelope = new EnvelopeClass();
            for (int i = 0; i < count; i++)
            {
                ipLayer = m_ipMap.get_Layer(i);
                ipFeatureLayer = ipLayer as IFeatureLayer;
                ipGeoDataset = ipFeatureLayer as IGeoDataset;
                ipEnvelope = ipGeoDataset.Extent;
                ipMaxEnvelope.Union(ipEnvelope);
            }
            m_ipPointToEID = new PointToEIDClass();
            m_ipPointToEID.SourceMap = m_ipMap;
            m_ipPointToEID.GeometricNetwork = m_ipGeometricNetwork;
            double dblWidth = ipMaxEnvelope.Width;
            double dblHeight = ipMaxEnvelope.Height;
            if (dblWidth > dblHeight)
                dblSearchTol = dblWidth / 100;
            else
                dblSearchTol = dblHeight / 100;
            m_ipPointToEID.SnapTolerance = dblSearchTol;
            return true;
        }

        private void CloseWorkspace()
        {
            m_ipGeometricNetwork = null;
            m_ipPoints = null;
            m_ipPointToEID = null;
            m_ipEnumNetEID_Junctions = null;
            m_ipEnumNetEID_Edges = null;
            m_ipPolyline = null;
        }
        #endregion
    }
}
