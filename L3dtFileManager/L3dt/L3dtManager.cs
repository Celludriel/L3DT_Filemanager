using L3dtFileManager.Amf;
using L3dtFileManager.Dmf;
using L3dtFileManager.Hff;
using L3dtFileManager.Hfz;
using L3dtFileManager.Wmf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace L3dtFileManager.L3dt
{
    public class L3dtManager
    {
        private HffManager hffManager = new HffManager();
        private HfzManager hfzManager = new HfzManager();
        private WmfManager wmfManager = new WmfManager();
        private AmfManager amfManager = new AmfManager();
        private DmfManager dmfManager = new DmfManager();

        public L3dtFile loadL3dtFile(string fileName)
        {
            L3dtFile retValue = new L3dtFile();


            using (StreamReader stream = new StreamReader(fileName))
            {
                XmlDocument projectFile = new XmlDocument();
                projectFile.Load(stream);

                LoadDmfMap(fileName, retValue, projectFile);
                LoadHeightMap(fileName, retValue, projectFile);
                LoadWmfWaterMap(fileName, retValue, projectFile);
                LoadAmfAttributeMap(fileName, retValue, projectFile);
                LoadClimateTable(retValue, projectFile);
            }

            return retValue;
        }

        private static void LoadClimateTable(L3dtFile retValue, XmlDocument projectFile)
        {
            XmlNodeList nodes = projectFile.SelectNodes("/varlist[@name='ProjectData']/varlist[@name='Climates']/varlist");
            if (nodes != null)
            {
                byte climateId = 0;
                foreach (XmlNode xmlNode in nodes)
                {
                    Stream climateFileStream = (Stream)File.Open(xmlNode.InnerText, FileMode.Open);
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(climateFileStream);
                    climateFileStream.Close();

                    byte landTypeId = 1;
                    foreach (XmlElement xmlElement in xmlDocument.SelectNodes("/varlist/varlist[@name='LandTypeList']/varlist"))
                    {
                        string key = climateId + "-" + landTypeId;
                        if (retValue.ClimateTable[key] == null)
                        {
                            retValue.ClimateTable.Add(key, xmlElement.Attributes["name"].Value);
                        }
                        landTypeId++;
                    }
                    climateId++;
                }
            }
        }

        private void LoadDmfMap(string fileName, L3dtFile retValue, XmlDocument projectFile)
        {
            XmlNode node = projectFile.SelectSingleNode("/varlist[@name='ProjectData']/varlist[@name='Maps']/varlist[@name='DM']/string[@name='Filename']");
            if (node != null)
            {
                string dmfFileName = fileName.Substring(0, fileName.LastIndexOf('\\') + 1) + node.InnerText;
                retValue.DmfFile = dmfManager.loadFile(dmfFileName);
            }
        }

        private void LoadAmfAttributeMap(string fileName, L3dtFile retValue, XmlDocument projectFile)
        {
            XmlNode node = projectFile.SelectSingleNode("/varlist[@name='ProjectData']/varlist[@name='Maps']/varlist[@name='AM']/string[@name='Filename']");
            if (node != null)
            {
                string amfFileName = fileName.Substring(0, fileName.LastIndexOf('\\') + 1) + node.InnerText;
                if (amfFileName.EndsWith(".amf") || amfFileName.EndsWith(".amf.gz"))
                {
                    retValue.AmfFile = amfFileName.EndsWith(".amf.gz") ? amfManager.loadFile(amfFileName, FileFormat.COMPRESSED) : amfManager.loadFile(amfFileName, FileFormat.UNCOMPRESSED);
                }
            }
        }

        private void LoadWmfWaterMap(string fileName, L3dtFile retValue, XmlDocument projectFile)
        {
            XmlNode node = projectFile.SelectSingleNode("/varlist[@name='ProjectData']/varlist[@name='Maps']/varlist[@name='WM']/string[@name='Filename']");
            if (node != null)
            {
                string waterMapFileName = fileName.Substring(0, fileName.LastIndexOf('\\') + 1) + node.InnerText;
                retValue.WmfFile = wmfManager.loadFile(waterMapFileName);
            }
        }

        private void LoadHeightMap(string fileName, L3dtFile retValue, XmlDocument projectFile)
        {
            XmlNode node = projectFile.SelectSingleNode("/varlist[@name='ProjectData']/varlist[@name='Maps']/varlist[@name='HF']/string[@name='Filename']");            
            if (node != null)
            {
                string heightMapFileName = fileName.Substring(0, fileName.LastIndexOf('\\') + 1) + node.InnerText;

                if (heightMapFileName.EndsWith("hfz") || heightMapFileName.EndsWith("hf2.gz") || heightMapFileName.EndsWith("hf2"))
                {
                    retValue.HfzFile = hfzManager.loadFile(heightMapFileName, heightMapFileName.EndsWith("hf2.gz") || heightMapFileName.EndsWith("hfz")  ? FileFormat.COMPRESSED : FileFormat.UNCOMPRESSED);
                }
                else if (heightMapFileName.EndsWith("hff"))
                {
                    retValue.HffFile = hffManager.loadFile(heightMapFileName);
                }
            }
        }
    }
}
