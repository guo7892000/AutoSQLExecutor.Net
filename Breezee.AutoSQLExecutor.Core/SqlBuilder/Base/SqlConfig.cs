using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Collections;
using System.Text.RegularExpressions;
using System.Diagnostics;

/***********************************************************
 * 对象名称：Sql配置类
 * 对象类别：共用方法类
 * 创建作者：黄国辉
 * 创建日期：2014-9-3
 * 对象说明：主要提供配置文件的读取、SQL的解析
 * 修改历史：
 *      V1.0 新建 huanggh 2014-9-3
 * ********************************************************/
namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// Sql辅助类
    /// </summary>
    public class SqlConfig
    {
        #region 全局变量
        //主SQL文件路径配置
        public static string SqlMainConfigPath = @"Config/sql.main.config";
        //包括的文件清单配置
        public static readonly string MainCofig_IncludeFile_Node = "include";
        public static readonly string MainCofig_IncludeFile_Node_Attribute = "uri";        
        /// <summary>
        /// 所有配置文件信息字典集合
        /// </summary>
        public static IDictionary<string, string> dicAllConfig = null;

        /// <summary>
        /// 配置文件路径
        /// </summary>
        private static string sConfigPath = "";

        #endregion      

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public SqlConfig()
        {
            InitConfigPath();
        }
        #endregion

        public static void InitConfigPath()
        {
            Assembly a = Assembly.GetExecutingAssembly();
            sConfigPath = a.CodeBase.Substring(8, a.CodeBase.LastIndexOf(@"/") - 7);
            sConfigPath = sConfigPath.Replace(@"/", @"\");
        }

        #region 递归遍历xml文件中所有节点信息
        /// <summary>
        /// 递归遍历xml文件中所有节点信息
        /// </summary>
        /// <author>黄国辉</author>
        /// <param name="xn"></param>
        /// <param name="sXMLPath"></param>
        /// <param name="dicDMSConfig"></param>
        public static void XmlFileAllNodes(XmlNode xn, string sXMLPath, IDictionary<string, string> dicDMSConfig)
        {
            if (xn.Attributes == null)
            {
                dicDMSConfig[sXMLPath] = xn.Value; // 设置XPath节点下的值
            }
            else
            {
                sXMLPath += "/" + xn.Name; // 合成XPath
                dicDMSConfig[sXMLPath] = xn.Value;
                // 对于xml文件中的文件的二次递归遍历
                for (int i = 0; i < xn.Attributes.Count; i++)
                {
                    if (xn.Name.Trim().Equals(MainCofig_IncludeFile_Node, StringComparison.OrdinalIgnoreCase) 
                        && xn.Attributes[i].Name.Trim().Equals(MainCofig_IncludeFile_Node_Attribute, StringComparison.OrdinalIgnoreCase) )
                    {
                        string sFileNAme = xn.Attributes[i].Value;
                        string sPrex = "file://";
                        sFileNAme = sFileNAme.Substring(sPrex.Length, sFileNAme.Length - sPrex.Length);

                        XmlDataDocument dmsXml = new XmlDataDocument();
                        try
                        {
                            dmsXml.Load(sConfigPath + "Config\\" + sFileNAme);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message + "  XMLPath:" + sConfigPath + "Config\\" + sFileNAme, ex);
                        }
                        string sXMLPath2 = dmsXml.DocumentElement.Name;

                        for (int j = 0; j < dmsXml.DocumentElement.ChildNodes.Count; j++)
                        {
                            XmlNode xnChild = dmsXml.DocumentElement.ChildNodes[j];
                            if (xnChild.NodeType == XmlNodeType.Comment)//hgh20210805
                            {
                                continue;
                            }
                            XmlFileAllNodes(xnChild, sXMLPath2, dicDMSConfig);
                        }
                    }
                }
            }

            string sValue = dicDMSConfig[sXMLPath];

            if (sValue != null)
            {
                sValue = sValue.Replace("&lt;", "<");// 对小于号的处理
                sValue = sValue.Replace("&gt;", ">"); // 对大于号的处理
                sValue = sValue.Replace("&amp;", "&"); // 对于符号的处理
                dicDMSConfig[sXMLPath] = sValue;
            }

            for (int i = 0; i < xn.ChildNodes.Count; i++)
            {
                XmlFileAllNodes(xn.ChildNodes[i], sXMLPath, dicDMSConfig);
            }
        }
        #endregion

        #region 取得特定配置项
        /// <summary>
        /// 取得特定配置项
        /// </summary>
        /// <author>黄国辉</author>
        /// <param name="sXPath">配置文件的XPath路径</param>
        /// <returns>返回特定配置项</returns>
        public static string GetGlobalConfigInfo(string sXPath)
        {
            try
            {
                // 合成全局配置
                if (dicAllConfig == null)
                {
                    dicAllConfig = new Dictionary<string, string>();
                    XmlDataDocument dmsXml = new XmlDataDocument();
                    if(string.IsNullOrEmpty(sConfigPath))
                    {
                        InitConfigPath();
                    }
                    //以下为配置根文件路径，必须保证有以下文件
                    dmsXml.Load(sConfigPath + SqlMainConfigPath); //所有SQL的主配置文件
                    string sXMLPath = dmsXml.DocumentElement.Name;

                    for (int i = 0; i < dmsXml.DocumentElement.ChildNodes.Count; i++)
                    {
                        XmlNode xn = dmsXml.DocumentElement.ChildNodes[i];
                        if (xn.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlFileAllNodes(xn, sXMLPath, dicAllConfig);
                    }
                }

                if (dicAllConfig.ContainsKey(sXPath))
                {
                    if (dicAllConfig[sXPath] == null)
                    {
                        dicAllConfig[sXPath] = "";
                    }

                    return dicAllConfig[sXPath];
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 获取指定路径的SQL
        /// <summary>
        /// 获取指定路径的SQL
        /// </summary>
        /// <author>黄国辉</author>
        /// <param name="sXPath">配置文件XPath路径</param>
        /// <param name="dicKeyValue">非SQL关键字的参数字典集合</param>
        /// <param name="dicSqlKeyWord">SQL关键字的参数字典集合：后台赋值</param>
        /// <returns>替换参数后可查询的SQL</returns>
        public static string GetSqlByPath(string sXPath)
        {
            try
            {
                // 取得当前配置的sql语句
                return GetGlobalConfigInfo(sXPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion


    }

}
