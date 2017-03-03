using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using Microsoft.VisualBasic.FileIO;

namespace SmartAlarmAgent.Model
{
     class AlarmList
        {
            private UInt32 m_nRecIndex;
            public UInt32 RecIndex
            {
                get { return m_nRecIndex; }
            }

            private UInt32 m_nPkAlarmListID;
            public UInt32 PkAlarmListID
            {
                get { return m_nPkAlarmListID; }
            }

            private DateTime m_DateTime;
            public DateTime Time
            {
                get { return m_DateTime; }
                set { m_DateTime = value; }
            }

            private PointType m_PointType;
            public PointType pointType
            {
                get { return m_PointType; }
            }

            private UInt32 m_nFkIndex;
            public UInt32 FkIndex
            {
                get { return m_nFkIndex; }
            }

            private String m_strStationName;
            public String StationName
            {
                get { return m_strStationName; }
            }

            private String m_strPointName;
            public String PointName
            {
                get { return m_strPointName; }
            }

            private UInt32 m_nAlarmType;
            public UInt32 AlarmType
            {
                get { return m_nAlarmType; }
            }

            private float m_fActualValue;
            public float ActualValue
            {
                get { return m_fActualValue; }
            }

            private String m_strMessage;
            public String Message
            {
                get { return m_strMessage; }
            }

            private Byte m_byFlashing;
            public Byte Flashing
            {
                get { return m_byFlashing; }
            }

            private String m_strSourceName;
            public String SourceName
            {
                get { return m_strSourceName; }
            }

            private Int32 m_nSourceID;
            public Int32 SourceID
            {
                get { return m_nSourceID; }
            }

            private Int32 m_nSourceType;
            public Int32 SourceType
            {
                get { return m_nSourceType; }
            }

            private Int32 m_nAlarmFlag;
            public Int32 AlarmFlag
            {
                get { return m_nAlarmFlag; }
            }

            private String m_strGroupPointName;
            public String GroupPointName
            {
                get { return m_strGroupPointName; }
            }

            private String m_strGroupDescription;
            public String GroupDescription
            {
                get { return m_strGroupDescription; }
            }

            private String m_strPriority;
            public String Priority
            {
                get { return m_strPriority; }
            }

            private bool m_bIsRestoration;
            public bool IsRestoration
            {
                get { return m_bIsRestoration; }
            }


            public AlarmList(DataRow parts)
            {
                try
                {
                    this.m_nRecIndex = UInt32.Parse(parts[(int)AlarmListTableField.RECINDEX_FIELD].ToString());
                    this.m_nPkAlarmListID = UInt32.Parse(parts[(int)AlarmListTableField.PKALARMLIST_FIELD].ToString());
                    this.m_DateTime = DateTime.ParseExact(parts[(int)AlarmListTableField.DATETIME_FIELD].ToString(), "dd/MM/yyyy HH:mm:ss.000", System.Globalization.CultureInfo.InvariantCulture);
                    this.m_PointType = (PointType)Byte.Parse(parts[(int)AlarmListTableField.POINTTYPE_FIELD].ToString());
                    this.m_nFkIndex = UInt32.Parse(parts[(int)AlarmListTableField.FKINDEX_FIELD].ToString());
                    this.m_strStationName = parts[(int)AlarmListTableField.STATIONNAME_FIELD].ToString();
                    this.m_strPointName = parts[(int)AlarmListTableField.POINTNAME_FIELD].ToString();
                    this.m_nAlarmType = UInt32.Parse(parts[(int)AlarmListTableField.ALARMTYPE_FIELD].ToString());
                    this.m_byFlashing = Byte.Parse(parts[(int)AlarmListTableField.FLASHING_FIELD].ToString());
                    this.m_fActualValue = float.Parse(parts[(int)AlarmListTableField.ACTUALVALUE_FIELD].ToString());
                    this.m_strMessage = parts[(int)AlarmListTableField.MESSAGE_FIELD].ToString();
                    this.m_strSourceName = parts[(int)AlarmListTableField.SOURCENAME_FIELD].ToString();
                    this.m_nSourceID = Int32.Parse(parts[(int)AlarmListTableField.SOURCEID_FIELD].ToString());
                    this.m_nSourceType = Int32.Parse(parts[(int)AlarmListTableField.SOURCETYPE_FIELD].ToString());
                    this.m_nAlarmFlag = Int32.Parse(parts[(int)AlarmListTableField.ALARMFLAG_FIELD].ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            public AlarmList()
            {
                //Null
            }

            public static AlarmList GetLineAlarmListCsv(string csvLine)
            {
#if false  //เกิดปัญหากับ Points : "NCO", "51-212,51-222,51-232 CONVERTER FAIL"
            string csnLineTemp = csvLine.Replace("\"", string.Empty); 
            string[] value = csnLineTemp.Split(new char[] { ',','"' });
#else
                List<string> iColumn = new List<string>();
                using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(csvLine)))
                {
                    using (TextFieldParser parser = new TextFieldParser(ms))
                    {

                        parser.Delimiters = new string[] { "," };
                        int iLine = 0;
                        while (true)
                        {
                            string[] parts = parser.ReadFields();
                            if (parts == null) break;
                            if (iLine++ == 0)
                            {
                                for (int iCol = 0; iCol < parts.Length; iCol++) iColumn.Add(parts[iCol]);
                                continue;
                            }
                        }
                    }
                }
#endif
            AlarmList localValue = new AlarmList();
                try
                {
                    localValue.m_nRecIndex = UInt32.Parse(iColumn[(int)AlarmListTableField.RECINDEX_FIELD].ToString());
                    localValue.m_nPkAlarmListID = UInt32.Parse(iColumn[(int)AlarmListTableField.PKALARMLIST_FIELD].ToString());
                    localValue.m_DateTime = DateTime.ParseExact(iColumn[(int)AlarmListTableField.DATETIME_FIELD].ToString(), "dd/MM/yyyy HH:mm:ss.000", System.Globalization.CultureInfo.InvariantCulture);
                    localValue.m_PointType = (PointType)Byte.Parse(iColumn[(int)AlarmListTableField.POINTTYPE_FIELD].ToString());
                    localValue.m_nFkIndex = UInt32.Parse(iColumn[(int)AlarmListTableField.FKINDEX_FIELD].ToString());
                    localValue.m_strStationName = iColumn[(int)AlarmListTableField.STATIONNAME_FIELD].ToString().Trim();
                    localValue.m_strPointName = iColumn[(int)AlarmListTableField.POINTNAME_FIELD].ToString().Trim();
                    localValue.m_nAlarmType = UInt32.Parse(iColumn[(int)AlarmListTableField.ALARMTYPE_FIELD].ToString());
                    localValue.m_byFlashing = Byte.Parse(iColumn[(int)AlarmListTableField.FLASHING_FIELD].ToString());
                    localValue.m_fActualValue = float.Parse(iColumn[(int)AlarmListTableField.ACTUALVALUE_FIELD].ToString());
                    localValue.m_strMessage = iColumn[(int)AlarmListTableField.MESSAGE_FIELD].ToString();
                    localValue.m_strSourceName = iColumn[(int)AlarmListTableField.SOURCENAME_FIELD].ToString();
                    localValue.m_nSourceID = Int32.Parse(iColumn[(int)AlarmListTableField.SOURCEID_FIELD].ToString());
                    localValue.m_nSourceType = Int32.Parse(iColumn[(int)AlarmListTableField.SOURCETYPE_FIELD].ToString());
                    localValue.m_nAlarmFlag = Int32.Parse(iColumn[(int)AlarmListTableField.ALARMFLAG_FIELD].ToString());
                    return localValue;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return null;
                }
            }
        }
 }


