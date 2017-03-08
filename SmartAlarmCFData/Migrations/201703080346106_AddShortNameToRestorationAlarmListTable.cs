namespace SmartAlarmCFData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddShortNameToRestorationAlarmListTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RestorationAlarmList", "ShortName", c => c.String(maxLength: 14));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RestorationAlarmList", "ShortName");
        }
    }
}
