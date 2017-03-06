namespace SmartAlarmAgent.Model
{
        public enum PointType
        {
            Digital,
            Analog
        };
        public enum AlarmListTableField
        {
            RECINDEX_FIELD = 0,
            PKALARMLIST_FIELD = 1,
            DATETIME_FIELD = 2,
            POINTTYPE_FIELD = 3,
            FKINDEX_FIELD = 4,
            STATIONNAME_FIELD = 5,
            POINTNAME_FIELD = 6,
            ALARMTYPE_FIELD = 7,
            FLASHING_FIELD = 8,
            ACTUALVALUE_FIELD = 9,
            MESSAGE_FIELD = 10,
            SOURCENAME_FIELD = 11,
            SOURCEID_FIELD = 12,
            SOURCETYPE_FIELD = 13,
            ALARMFLAG_FIELD = 14
        };

    public enum EventLogPosition
    {
        CSV_STATUS,
        CSV_NEW_EVENT,
        REST_NEW_POINT,
        SEP_1,
        DB_STATUS,
        DB_TOTAL_POINT,
        DB_LAST_ACCESS,
        SEP_2,
        MSG_TITLE,
        ERR_MSG,
        ETC_STATUS
    }
    
}
