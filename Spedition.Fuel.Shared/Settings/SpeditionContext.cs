using Spedition.Fuel.Shared.Entities;

namespace Spedition.Fuel.Shared.Settings;

public class SpeditionContext : DbContext
{
    public SpeditionContext(DbContextOptions<SpeditionContext> options)
        : base(options)
    {
    }

    public virtual DbSet<FuelCard> Cards { get; set; }

    public virtual DbSet<FuelTransaction> Transactions { get; set; }

    public virtual DbSet<FuelProvider> Providers { get; set; }

    public virtual DbSet<FuelCardsEvent> CardsEvents { get; set; }

    public virtual DbSet<FuelCardsAlternativeNumber> CardsAlternativeNumbers { get; set; }

    public virtual DbSet<FuelCardsCountry> CardsCountries { get; set; }

    public virtual DbSet<FuelType> FuelTypes { get; set; }

    public virtual DbSet<NotFoundFuelCard> NotFoundCard { get; set; }

    public virtual DbSet<ProvidersAccount> CardsAccounts { get; set; }

    public virtual DbSet<BPTransaction> BPTransactions { get; set; }

    public virtual DbSet<KitEventType> KitEventTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("Relational:Collation", "Cyrillic_General_CI_AS");

        modelBuilder.Entity<KitEventType>(entity =>
        {
            entity.ToTable("kit_event_types", "dbo");
        });

        modelBuilder.Entity<BPTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BPTransa__3213E83F582FACD1");

            entity.ToTable("BPTransactions", "fuel");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.AuthorityId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("authority_id");
            entity.Property(e => e.BunkPriceInd)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("bunk_price_ind");
            entity.Property(e => e.CardHolderName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("card_holder_name");
            entity.Property(e => e.CardSerialNo).HasColumnName("card_serial_no");
            entity.Property(e => e.Cc2)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("cc2");
            entity.Property(e => e.CostCentreName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("cost_centre_name");
            entity.Property(e => e.CostCentreNo).HasColumnName("cost_centre_no");
            entity.Property(e => e.CountryCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("country_code");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("currency");
            entity.Property(e => e.CustomerId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("customer_id");
            entity.Property(e => e.FeeItemCount).HasColumnName("fee_item_count");
            entity.Property(e => e.GrossRebate)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("gross_rebate");
            entity.Property(e => e.GrossValue)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("gross_value");
            entity.Property(e => e.IccInvoiceNo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("icc_invoice_no");
            entity.Property(e => e.Info)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("info");
            entity.Property(e => e.InvoiceNo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("invoice_no");
            entity.Property(e => e.InvoiceNotifyDate)
                .HasColumnType("datetime")
                .HasColumnName("invoice_notify_date");
            entity.Property(e => e.InvoiceType).HasColumnName("invoice_type");
            entity.Property(e => e.IssueNo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("issue_no");
            entity.Property(e => e.LocalCurrency)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("local_currency");
            entity.Property(e => e.LosName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("los_name");
            entity.Property(e => e.LosNo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("los_no");
            entity.Property(e => e.LosType).HasColumnName("los_type");
            entity.Property(e => e.NettoValue)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("netto_value");
            entity.Property(e => e.Odometer).HasColumnName("odometer");
            entity.Property(e => e.OpuCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("opu_code");
            entity.Property(e => e.OriginalCountryCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("original_country_code");
            entity.Property(e => e.OriginalCurrency)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("original_currency");
            entity.Property(e => e.OriginalGrossAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("original_gross_amount");
            entity.Property(e => e.OriginalNettoAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("original_netto_amount");
            entity.Property(e => e.OriginalValue).HasColumnName("original_value");
            entity.Property(e => e.OriginalVatAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("original_vat_amount");
            entity.Property(e => e.OriginalVatRate)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("original_vat_rate");
            entity.Property(e => e.ParentId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("parent_id");
            entity.Property(e => e.PickupType)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("pickup_type");
            entity.Property(e => e.PriceAreaCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("price_area_code");
            entity.Property(e => e.ProdCat)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("prod_cat");
            entity.Property(e => e.ProdCode).HasColumnName("prod_code");
            entity.Property(e => e.ProdDesc)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("prod_desc");
            entity.Property(e => e.Quantity)
                .HasColumnType("decimal(10, 3)")
                .HasColumnName("quantity");
            entity.Property(e => e.ReverseCharge)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("reverse_charge");
            entity.Property(e => e.ScheduledUnitPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("scheduled_unit_price");
            entity.Property(e => e.StateCurrency)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("state_currency");
            entity.Property(e => e.SubCostCentre)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("sub_cost_centre");
            entity.Property(e => e.SupplierGrossRebate)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("supplier_gross_rebate");
            entity.Property(e => e.SupplierGrossValue)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("supplier_gross_value");
            entity.Property(e => e.SupplierNettoValue)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("supplier_netto_value");
            entity.Property(e => e.SupplierUnitPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("supplier_unit_price");
            entity.Property(e => e.SupplierVatRate)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("supplier_vat_rate");
            entity.Property(e => e.SupplierVatValue)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("supplier_vat_value");
            entity.Property(e => e.TransactionKey)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("transaction_key");
            entity.Property(e => e.TxnDatetime)
                .HasColumnType("datetime")
                .HasColumnName("txn_datetime");
            entity.Property(e => e.TxnId2)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("txn_id_2");
            entity.Property(e => e.TxnNo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("txn_no");
            entity.Property(e => e.TxnWorkday)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("txn_workday");
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(10, 3)")
                .HasColumnName("unit_price");
            entity.Property(e => e.VatRate)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("vat_rate");
            entity.Property(e => e.VatValue)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("vat_value");
            entity.Property(e => e.VehDriverCode).HasColumnName("veh_driver_code");
            entity.Property(e => e.VehicleReg)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("vehicle_reg");
            entity.Property(e => e.VentPart)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("vent_part");
        });

        modelBuilder.Entity<FuelTransaction>(entity =>
        {
            entity.ToTable("FuelCardsEntries", "dbo", _ =>
            {
                _.HasTrigger("History_FuelCardsEntries_trd");
                _.HasTrigger("History_FuelCardsEntries_tri");
                _.HasTrigger("History_FuelCardsEntries_tru");
            });

            entity.Property(e => e.Id).HasColumnName("ID");

            entity.Property(e => e.CardId).HasColumnName("CardID");

            entity.Property(e => e.ProviderId).HasColumnName("CardTypeID");

            entity.Property(e => e.Cost).HasColumnType("money");

            entity.Property(e => e.CountryId).HasColumnName("CountryID");

            entity.Property(e => e.CurrencyId).HasColumnName("CurrencyID");

            entity.Property(e => e.DriverReportId).HasColumnName("DriverReportID ");

            entity.Property(e => e.OperationDate).HasColumnType("datetime");

            entity.Property(e => e.Quantity).HasColumnType("money");

            entity.Property(e => e.TotalCost).HasColumnType("money");

            entity.Property(e => e.TransactionID)
                .HasMaxLength(255)
                .HasColumnName("TransactionID");
        });

        modelBuilder.Entity<FuelCard>(entity =>
        {
            entity.ToTable("tCarFuelCards", "dbo", _ =>
            {
                _.HasTrigger("History_tCarFuelCards_trd");
                _.HasTrigger("History_tCarFuelCards_tri");
                _.HasTrigger("History_tCarFuelCards_tru");
            });

            entity.Property(e => e.Id).HasColumnName("ID");

            entity.Property(e => e.DivisionID).HasColumnName("DivisionID");

            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeId");

            entity.Property(e => e.ExpirationDate).HasColumnType("datetime");

            entity.Property(e => e.CarId).HasColumnName("FK_CarID");

            entity.Property(e => e.ProviderId).HasColumnName("FK_FuelCardTypeID");

            entity.Property(e => e.IssueDate).HasColumnType("datetime");

            entity.Property(e => e.Note).HasMaxLength(2048);

            entity.Property(e => e.Number)
                .IsRequired()
                .HasMaxLength(25);

            entity.Property(e => e.ReceiveDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<FuelCardsEvent>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK_fuel_card_events_event_id");

            entity.ToTable("fuel_card_events", "dbo", _ =>
            {
                _.HasTrigger("tr_fuel_card_events_change");
                _.HasTrigger("tr_fuel_card_events_instead");
            });

            entity.Property(e => e.Id).HasColumnName("event_id");

            entity.Property(e => e.CarId).HasColumnName("car_id");

            entity.Property(e => e.CardId).HasColumnName("card_id");

            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("change_date");

            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("change_username");

            entity.Property(e => e.Comment)
                .HasMaxLength(250)
                .HasColumnName("comment");

            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("event_start_date");

            entity.Property(e => e.EventTypeId).HasColumnName("event_type_id");

            entity.Property(e => e.FinishDate)
                .HasColumnType("datetime")
                .HasColumnName("finish_date");

            entity.HasOne(d => d.Card)
                .WithMany(p => p.FuelCardsEvents)
                .HasForeignKey(d => d.CardId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_fuel_card_events_card_id");
        });

        modelBuilder.Entity<FuelCardsAlternativeNumber>(entity =>
        {
            entity.ToTable(tb =>
            {
                tb.HasComment("Дубликат номера топливной карты - из отчета по топливу");
                tb.HasTrigger("tr_FuelCardsAlternativeNumbers_change");
                tb.HasTrigger("tr_FuelCardsAlternativeNumbers_instead");
            });

            entity.Property(e => e.Number)
                .IsRequired()
                .HasMaxLength(25);

            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("change_date");

            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("change_username");

            entity.HasOne(d => d.Card)
               .WithMany(p => p.FuelCardsAlternativeNumbers)
               .HasForeignKey(d => d.CardId)
               .OnDelete(DeleteBehavior.NoAction)
               .HasConstraintName("FK_FuelCardsAlternativeNumbers_FuelCardId");
        });

        base.OnModelCreating(modelBuilder);
    }
}
