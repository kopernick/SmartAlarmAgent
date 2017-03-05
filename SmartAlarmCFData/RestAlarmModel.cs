namespace SmartAlarmCFData
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class RestAlarmDbContext : DbContext
    {
        public RestAlarmDbContext()
            : base("name=RestAlarmDbContext")
        {
        }

        public virtual DbSet<Device> Devices { get; set; }
        public virtual DbSet<DigitalPointInfo> DigitalPointInfoes { get; set; }
        public virtual DbSet<Priority> Priorities { get; set; }
        public virtual DbSet<RestorationAlarmList> RestorationAlarmLists { get; set; }
        public virtual DbSet<Station> Stations { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Device>()
                .Property(e => e.DeviceType)
                .IsUnicode(false);

            modelBuilder.Entity<DigitalPointInfo>()
                .Property(e => e.StationName)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<DigitalPointInfo>()
                .Property(e => e.PointName)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<DigitalPointInfo>()
                .Property(e => e.ShortName)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<DigitalPointInfo>()
                .Property(e => e.DeviceType)
                .IsUnicode(false);

            modelBuilder.Entity<DigitalPointInfo>()
                .Property(e => e.MACName)
                .IsUnicode(false);

            modelBuilder.Entity<DigitalPointInfo>()
                .Property(e => e.Priority)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Priority>()
                .Property(e => e.PriorityName)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<RestorationAlarmList>()
                .Property(e => e.StationName)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<RestorationAlarmList>()
                .Property(e => e.PointName)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<RestorationAlarmList>()
                .Property(e => e.Message)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<RestorationAlarmList>()
                .Property(e => e.SourceName)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<RestorationAlarmList>()
                .Property(e => e.DeviceType)
                .IsUnicode(false);

            modelBuilder.Entity<RestorationAlarmList>()
                .Property(e => e.MACName)
                .IsUnicode(false);

            modelBuilder.Entity<RestorationAlarmList>()
                .Property(e => e.Priority)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Station>()
                .Property(e => e.StationName)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Station>()
                .Property(e => e.Detail)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Station>()
                .Property(e => e.DCSName)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Station>()
                .Property(e => e.RegionName)
                .IsFixedLength()
                .IsUnicode(false);
        }
    }
}
