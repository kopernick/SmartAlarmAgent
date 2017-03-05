namespace SmartAlarmCFData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeMACNameLengToRestorationAlarmListTable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.RestorationAlarmList", "MACName", c => c.String(maxLength: 255, unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.RestorationAlarmList", "MACName", c => c.String(maxLength: 100, unicode: false));
        }
    }
}
