using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Models;

public partial class PizzaShopContext : DbContext
{
    public PizzaShopContext(DbContextOptions<PizzaShopContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Dish> Dishes { get; set; }

    public virtual DbSet<Dishmodifier> Dishmodifiers { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Invoicetax> Invoicetaxes { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<ItemsUnit> ItemsUnits { get; set; }

    public virtual DbSet<KotTable> KotTables { get; set; }

    public virtual DbSet<MapItemsModifiersgroup> MapItemsModifiersgroups { get; set; }

    public virtual DbSet<MapModifiersgroupModifier> MapModifiersgroupModifiers { get; set; }

    public virtual DbSet<MapOrderTable> MapOrderTables { get; set; }

    public virtual DbSet<MapTableToken> MapTableTokens { get; set; }

    public virtual DbSet<Modifier> Modifiers { get; set; }

    public virtual DbSet<Modifiersgroup> Modifiersgroups { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Orderapp> Orderapps { get; set; }

    public virtual DbSet<Orderstatus> Orderstatuses { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Permissionlist> Permissionlists { get; set; }

    public virtual DbSet<Resettoken> Resettokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<Table> Tables { get; set; }

    public virtual DbSet<Taxis> Taxes { get; set; }

    public virtual DbSet<Totalrating> Totalratings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WaitingTable> WaitingTables { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Categoryid).HasName("category_pkey");

            entity.ToTable("category");

            entity.HasIndex(e => e.Categoryname, "category_categoryname_key").IsUnique();

            entity.Property(e => e.Categoryid).HasColumnName("categoryid");
            entity.Property(e => e.Categorydescription)
                .HasMaxLength(100)
                .HasColumnName("categorydescription");
            entity.Property(e => e.Categoryname)
                .HasMaxLength(20)
                .HasColumnName("categoryname");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.EditDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("edit_date");
            entity.Property(e => e.EditedBy)
                .HasMaxLength(50)
                .HasColumnName("edited_by");
            entity.Property(e => e.Isdeleted)
                .HasDefaultValueSql("false")
                .HasColumnName("isdeleted");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.Cityid).HasName("city_pkey");

            entity.ToTable("city");

            entity.Property(e => e.Cityid).HasColumnName("cityid");
            entity.Property(e => e.Cityname)
                .HasMaxLength(50)
                .HasColumnName("cityname");
            entity.Property(e => e.Stateid).HasColumnName("stateid");

            entity.HasOne(d => d.State).WithMany(p => p.Cities)
                .HasForeignKey(d => d.Stateid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("city_stateid_fkey");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Countryid).HasName("country_pkey");

            entity.ToTable("country");

            entity.Property(e => e.Countryid).HasColumnName("countryid");
            entity.Property(e => e.Countryname)
                .HasMaxLength(50)
                .HasColumnName("countryname");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Customerid).HasName("customers_pkey");

            entity.ToTable("customers");

            entity.HasIndex(e => e.Customeremail, "customers_customeremail_key").IsUnique();

            entity.Property(e => e.Customerid).HasColumnName("customerid");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Customeremail)
                .HasMaxLength(50)
                .HasColumnName("customeremail");
            entity.Property(e => e.Customername)
                .HasMaxLength(50)
                .HasColumnName("customername");
            entity.Property(e => e.EditDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("edit_date");
            entity.Property(e => e.EditedBy)
                .HasMaxLength(50)
                .HasColumnName("edited_by");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("phone_number");
        });

        modelBuilder.Entity<Dish>(entity =>
        {
            entity.HasKey(e => e.Dishid).HasName("dish_pkey");

            entity.ToTable("dish");

            entity.Property(e => e.Dishid).HasColumnName("dishid");
            entity.Property(e => e.Inprogressquantity)
                .HasDefaultValueSql("0")
                .HasColumnName("inprogressquantity");
            entity.Property(e => e.Inservedquantity)
                .HasDefaultValueSql("0")
                .HasColumnName("inservedquantity");
            entity.Property(e => e.Itemid).HasColumnName("itemid");
            entity.Property(e => e.Iteminstruction).HasColumnName("iteminstruction");
            entity.Property(e => e.Itemname)
                .HasColumnType("character varying")
                .HasColumnName("itemname");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Pendingquantity).HasColumnName("pendingquantity");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Readyquantity)
                .HasDefaultValueSql("0")
                .HasColumnName("readyquantity");

            entity.HasOne(d => d.Item).WithMany(p => p.Dishes)
                .HasForeignKey(d => d.Itemid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dish_itemid_fkey");

            entity.HasOne(d => d.Order).WithMany(p => p.Dishes)
                .HasForeignKey(d => d.Orderid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dish_orderid_fkey");
        });

        modelBuilder.Entity<Dishmodifier>(entity =>
        {
            entity.HasKey(e => e.Dishmodifiersid).HasName("dishmodifiers_pkey");

            entity.ToTable("dishmodifiers");

            entity.Property(e => e.Dishmodifiersid).HasColumnName("dishmodifiersid");
            entity.Property(e => e.Dishid).HasColumnName("dishid");
            entity.Property(e => e.Modifierid).HasColumnName("modifierid");
            entity.Property(e => e.Modifiername)
                .HasColumnType("character varying")
                .HasColumnName("modifiername");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Dish).WithMany(p => p.Dishmodifiers)
                .HasForeignKey(d => d.Dishid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dishmodifiers_dishid_fkey");

            entity.HasOne(d => d.Modifier).WithMany(p => p.Dishmodifiers)
                .HasForeignKey(d => d.Modifierid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dishmodifiers_modifierid_fkey");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Invoiceid).HasName("invoice_pkey");

            entity.ToTable("invoice");

            entity.HasIndex(e => e.Invoicenumber, "invoice_invoicenumber_key").IsUnique();

            entity.Property(e => e.Invoiceid).HasColumnName("invoiceid");
            entity.Property(e => e.Invoicenumber)
                .HasMaxLength(150)
                .HasColumnName("invoicenumber");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Paidon)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("paidon");

            entity.HasOne(d => d.Order).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.Orderid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("invoice_orderid_fkey");
        });

        modelBuilder.Entity<Invoicetax>(entity =>
        {
            entity.HasKey(e => e.Invoicetaxid).HasName("invoicetax_pkey");

            entity.ToTable("invoicetax");

            entity.Property(e => e.Invoicetaxid).HasColumnName("invoicetaxid");
            entity.Property(e => e.Invoiceid).HasColumnName("invoiceid");
            entity.Property(e => e.Taxid).HasColumnName("taxid");
            entity.Property(e => e.Taxname)
                .HasMaxLength(255)
                .HasColumnName("taxname");
            entity.Property(e => e.Taxvalue)
                .HasPrecision(10, 2)
                .HasColumnName("taxvalue");
            entity.Property(e => e.Taxvaluetype)
                .HasMaxLength(10)
                .HasColumnName("taxvaluetype");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Invoicetaxes)
                .HasForeignKey(d => d.Invoiceid)
                .HasConstraintName("invoicetax_invoiceid_fkey");

            entity.HasOne(d => d.Tax).WithMany(p => p.Invoicetaxes)
                .HasForeignKey(d => d.Taxid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("invoicetax_taxid_fkey");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Itemid).HasName("items_pkey");

            entity.ToTable("items");

            entity.HasIndex(e => e.Itemname, "itemname").IsUnique();

            entity.Property(e => e.Itemid).HasColumnName("itemid");
            entity.Property(e => e.Categoryid).HasColumnName("categoryid");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Defaulttax)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("defaulttax");
            entity.Property(e => e.EditDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("edit_date");
            entity.Property(e => e.EditedBy)
                .HasMaxLength(50)
                .HasColumnName("edited_by");
            entity.Property(e => e.Isavailable)
                .HasDefaultValueSql("true")
                .HasColumnName("isavailable");
            entity.Property(e => e.Isdeleted)
                .HasDefaultValueSql("false")
                .HasColumnName("isdeleted");
            entity.Property(e => e.Isfavourite)
                .HasDefaultValueSql("false")
                .HasColumnName("isfavourite");
            entity.Property(e => e.Itemdescription)
                .HasMaxLength(100)
                .HasColumnName("itemdescription");
            entity.Property(e => e.Itemimage)
                .HasMaxLength(255)
                .HasColumnName("itemimage");
            entity.Property(e => e.Itemname)
                .HasMaxLength(100)
                .HasColumnName("itemname");
            entity.Property(e => e.Itemtype)
                .HasMaxLength(20)
                .HasColumnName("itemtype");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Rate).HasColumnName("rate");
            entity.Property(e => e.Shortcode)
                .HasMaxLength(10)
                .HasColumnName("shortcode");
            entity.Property(e => e.Taxesid).HasColumnName("taxesid");
            entity.Property(e => e.Taxpercentage)
                .HasPrecision(5, 2)
                .HasColumnName("taxpercentage");
            entity.Property(e => e.Unitid).HasColumnName("unitid");

            entity.HasOne(d => d.Category).WithMany(p => p.Items)
                .HasForeignKey(d => d.Categoryid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("items_categoryid_fkey");

            entity.HasOne(d => d.Taxes).WithMany(p => p.Items)
                .HasForeignKey(d => d.Taxesid)
                .HasConstraintName("items_taxesid_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.Items)
                .HasForeignKey(d => d.Unitid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("items_unitid_fkey");
        });

        modelBuilder.Entity<ItemsUnit>(entity =>
        {
            entity.HasKey(e => e.Unitid).HasName("items_unit_pkey");

            entity.ToTable("items_unit");

            entity.Property(e => e.Unitid).HasColumnName("unitid");
            entity.Property(e => e.Unitname)
                .HasMaxLength(50)
                .HasColumnName("unitname");
        });

        modelBuilder.Entity<KotTable>(entity =>
        {
            entity.HasKey(e => e.Kotid).HasName("kot_table_pkey");

            entity.ToTable("kot_table");

            entity.Property(e => e.Kotid).HasColumnName("kotid");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.EditDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("edit_date");
            entity.Property(e => e.EditedBy)
                .HasMaxLength(50)
                .HasColumnName("edited_by");
            entity.Property(e => e.ItemStatus).HasColumnName("item_status");
            entity.Property(e => e.OrderStatus).HasColumnName("order_status");
            entity.Property(e => e.Orderappid).HasColumnName("orderappid");

            entity.HasOne(d => d.Orderapp).WithMany(p => p.KotTables)
                .HasForeignKey(d => d.Orderappid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("kot_table_orderappid_fkey");
        });

        modelBuilder.Entity<MapItemsModifiersgroup>(entity =>
        {
            entity.HasKey(e => e.Mergrid).HasName("map_items_modifiersgroup_pkey");

            entity.ToTable("map_items_modifiersgroup");

            entity.Property(e => e.Mergrid).HasColumnName("mergrid");
            entity.Property(e => e.Itemid).HasColumnName("itemid");
            entity.Property(e => e.Maximum).HasColumnName("maximum");
            entity.Property(e => e.Minimum).HasColumnName("minimum");
            entity.Property(e => e.Modifiersgroupid).HasColumnName("modifiersgroupid");

            entity.HasOne(d => d.Item).WithMany(p => p.MapItemsModifiersgroups)
                .HasForeignKey(d => d.Itemid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("map_items_modifiersgroup_itemid_fkey");

            entity.HasOne(d => d.Modifiersgroup).WithMany(p => p.MapItemsModifiersgroups)
                .HasForeignKey(d => d.Modifiersgroupid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("map_items_modifiersgroup_modifiersgroupid_fkey");
        });

        modelBuilder.Entity<MapModifiersgroupModifier>(entity =>
        {
            entity.HasKey(e => e.Mergrid).HasName("map_modifiersgroup_modifiers_pkey");

            entity.ToTable("map_modifiersgroup_modifiers");

            entity.Property(e => e.Mergrid).HasColumnName("mergrid");
            entity.Property(e => e.Modifiersgroupid).HasColumnName("modifiersgroupid");
            entity.Property(e => e.Modifiersid).HasColumnName("modifiersid");

            entity.HasOne(d => d.Modifiersgroup).WithMany(p => p.MapModifiersgroupModifiers)
                .HasForeignKey(d => d.Modifiersgroupid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("map_modifiersgroup_modifiers_modifiersgroupid_fkey");

            entity.HasOne(d => d.Modifiers).WithMany(p => p.MapModifiersgroupModifiers)
                .HasForeignKey(d => d.Modifiersid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("map_modifiersgroup_modifiers_modifiersid_fkey");
        });

        modelBuilder.Entity<MapOrderTable>(entity =>
        {
            entity.HasKey(e => e.Mergrid).HasName("merge_table_pkey");

            entity.ToTable("map_order_table");

            entity.Property(e => e.Mergrid)
                .HasDefaultValueSql("nextval('merge_table_mergrid_seq'::regclass)")
                .HasColumnName("mergrid");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Tablesid).HasColumnName("tablesid");

            entity.HasOne(d => d.Order).WithMany(p => p.MapOrderTables)
                .HasForeignKey(d => d.Orderid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("merge_table_orderid_fkey");

            entity.HasOne(d => d.Tables).WithMany(p => p.MapOrderTables)
                .HasForeignKey(d => d.Tablesid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("merge_table_tablesid_fkey");
        });

        modelBuilder.Entity<MapTableToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("map_table_token_pkey");

            entity.ToTable("map_table_token");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Tableid).HasColumnName("tableid");
            entity.Property(e => e.Tokenid).HasColumnName("tokenid");

            entity.HasOne(d => d.Table).WithMany(p => p.MapTableTokens)
                .HasForeignKey(d => d.Tableid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("map_table_token_tableid_fkey");

            entity.HasOne(d => d.Token).WithMany(p => p.MapTableTokens)
                .HasForeignKey(d => d.Tokenid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("map_table_token_tokenid_fkey");
        });

        modelBuilder.Entity<Modifier>(entity =>
        {
            entity.HasKey(e => e.Modifiersid).HasName("modifiers_pkey");

            entity.ToTable("modifiers");

            entity.Property(e => e.Modifiersid).HasColumnName("modifiersid");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.EditDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("edit_date");
            entity.Property(e => e.EditedBy)
                .HasMaxLength(50)
                .HasColumnName("edited_by");
            entity.Property(e => e.Isdeleted)
                .HasDefaultValueSql("false")
                .HasColumnName("isdeleted");
            entity.Property(e => e.Modifiersdescription)
                .HasMaxLength(100)
                .HasColumnName("modifiersdescription");
            entity.Property(e => e.Modifiersname)
                .HasMaxLength(50)
                .HasColumnName("modifiersname");
            entity.Property(e => e.Modifiersquantity).HasColumnName("modifiersquantity");
            entity.Property(e => e.Modifiersrate).HasColumnName("modifiersrate");
            entity.Property(e => e.Modifiersunit).HasColumnName("modifiersunit");

            entity.HasOne(d => d.ModifiersunitNavigation).WithMany(p => p.Modifiers)
                .HasForeignKey(d => d.Modifiersunit)
                .HasConstraintName("modifiers_modifiersunit_fkey");
        });

        modelBuilder.Entity<Modifiersgroup>(entity =>
        {
            entity.HasKey(e => e.Modifiersgroupid).HasName("modifiersgroup_pkey");

            entity.ToTable("modifiersgroup");

            entity.HasIndex(e => e.Modifiersgroupname, "modifiersgroup_modifiersgroupname_key").IsUnique();

            entity.Property(e => e.Modifiersgroupid).HasColumnName("modifiersgroupid");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.EditDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("edit_date");
            entity.Property(e => e.EditedBy)
                .HasMaxLength(50)
                .HasColumnName("edited_by");
            entity.Property(e => e.Isdeleted)
                .HasDefaultValueSql("false")
                .HasColumnName("isdeleted");
            entity.Property(e => e.Modifiersgroupdescription)
                .HasMaxLength(100)
                .HasColumnName("modifiersgroupdescription");
            entity.Property(e => e.Modifiersgroupname)
                .HasMaxLength(20)
                .HasColumnName("modifiersgroupname");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Orderid).HasName("Order_pkey");

            entity.ToTable("Order");

            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.Completedtime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completedtime");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Customerid).HasColumnName("customerid");
            entity.Property(e => e.EditDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("edit_date");
            entity.Property(e => e.EditedBy)
                .HasMaxLength(50)
                .HasColumnName("edited_by");
            entity.Property(e => e.Instruction).HasColumnName("instruction");
            entity.Property(e => e.Orderstatusid).HasColumnName("orderstatusid");
            entity.Property(e => e.Paymentid).HasColumnName("paymentid");
            entity.Property(e => e.Personcount).HasColumnName("personcount");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Servetime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("servetime");
            entity.Property(e => e.Totalamount)
                .HasPrecision(10, 2)
                .HasColumnName("totalamount");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Customerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Order_customerid_fkey");

            entity.HasOne(d => d.Orderstatus).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Orderstatusid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Order_orderstatusid_fkey");

            entity.HasOne(d => d.Payment).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Paymentid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Order_paymentid_fkey");
        });

        modelBuilder.Entity<Orderapp>(entity =>
        {
            entity.HasKey(e => e.Orderappid).HasName("orderapp_pkey");

            entity.ToTable("orderapp");

            entity.Property(e => e.Orderappid).HasColumnName("orderappid");
            entity.Property(e => e.Comment)
                .HasMaxLength(255)
                .HasColumnName("comment");
            entity.Property(e => e.ItemComment)
                .HasMaxLength(255)
                .HasColumnName("item_comment");
            entity.Property(e => e.Itemid).HasColumnName("itemid");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Item).WithMany(p => p.Orderapps)
                .HasForeignKey(d => d.Itemid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orderapp_itemid_fkey");

            entity.HasOne(d => d.Order).WithMany(p => p.Orderapps)
                .HasForeignKey(d => d.Orderid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orderapp_orderid_fkey");
        });

        modelBuilder.Entity<Orderstatus>(entity =>
        {
            entity.HasKey(e => e.Orderstatusid).HasName("orderstatus_pkey");

            entity.ToTable("orderstatus");

            entity.Property(e => e.Orderstatusid).HasColumnName("orderstatusid");
            entity.Property(e => e.Statusname)
                .HasMaxLength(50)
                .HasColumnName("statusname");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Paymentid).HasName("payment_pkey");

            entity.ToTable("payment");

            entity.Property(e => e.Paymentid).HasColumnName("paymentid");
            entity.Property(e => e.Paymentmode)
                .HasMaxLength(20)
                .HasColumnName("paymentmode");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("permission_pkey");

            entity.ToTable("permission");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('permission_permissionid_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Canaddedit)
                .HasDefaultValueSql("true")
                .HasColumnName("canaddedit");
            entity.Property(e => e.Candelete)
                .HasDefaultValueSql("true")
                .HasColumnName("candelete");
            entity.Property(e => e.Canview)
                .HasDefaultValueSql("true")
                .HasColumnName("canview");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.EditDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("edit_date");
            entity.Property(e => e.EditedBy)
                .HasMaxLength(50)
                .HasColumnName("edited_by");
            entity.Property(e => e.IsEnable).HasDefaultValueSql("true");
            entity.Property(e => e.Permissionid).HasColumnName("permissionid");
            entity.Property(e => e.Roleid).HasColumnName("roleid");

            entity.HasOne(d => d.PermissionNavigation).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.Permissionid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("permissionlist_roleid_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.Roleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("permission_roleid_fkey");
        });

        modelBuilder.Entity<Permissionlist>(entity =>
        {
            entity.HasKey(e => e.Permissionid).HasName("permissionlist_pkey");

            entity.ToTable("permissionlist");

            entity.Property(e => e.Permissionid).HasColumnName("permissionid");
            entity.Property(e => e.Permissionname)
                .HasMaxLength(50)
                .HasColumnName("permissionname");
        });

        modelBuilder.Entity<Resettoken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("resettoken_pkey");

            entity.ToTable("resettoken");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdtime)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdtime");
            entity.Property(e => e.Token).HasColumnName("token");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Roleid).HasName("role_pkey");

            entity.ToTable("role");

            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.EditDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("edit_date");
            entity.Property(e => e.EditedBy)
                .HasMaxLength(50)
                .HasColumnName("edited_by");
            entity.Property(e => e.Rolename)
                .HasMaxLength(50)
                .HasColumnName("rolename");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.Sectionid).HasName("section_pkey");

            entity.ToTable("section");

            entity.HasIndex(e => e.Sectionname, "section_sectionname_key").IsUnique();

            entity.Property(e => e.Sectionid).HasColumnName("sectionid");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.EditDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("edit_date");
            entity.Property(e => e.EditedBy)
                .HasMaxLength(50)
                .HasColumnName("edited_by");
            entity.Property(e => e.Isdeleted)
                .HasDefaultValueSql("false")
                .HasColumnName("isdeleted");
            entity.Property(e => e.Sectiondescription)
                .HasMaxLength(100)
                .HasColumnName("sectiondescription");
            entity.Property(e => e.Sectionname)
                .HasMaxLength(20)
                .HasColumnName("sectionname");
        });

        modelBuilder.Entity<State>(entity =>
        {
            entity.HasKey(e => e.Stateid).HasName("state_pkey");

            entity.ToTable("state");

            entity.Property(e => e.Stateid).HasColumnName("stateid");
            entity.Property(e => e.Countryid).HasColumnName("countryid");
            entity.Property(e => e.Statename)
                .HasMaxLength(50)
                .HasColumnName("statename");

            entity.HasOne(d => d.Country).WithMany(p => p.States)
                .HasForeignKey(d => d.Countryid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("state_countryid_fkey");
        });

        modelBuilder.Entity<Table>(entity =>
        {
            entity.HasKey(e => e.Tablesid).HasName("tables_pkey");

            entity.ToTable("tables");

            entity.Property(e => e.Tablesid).HasColumnName("tablesid");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Currenttokenid).HasColumnName("currenttokenid");
            entity.Property(e => e.EditDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("edit_date");
            entity.Property(e => e.EditedBy)
                .HasMaxLength(50)
                .HasColumnName("edited_by");
            entity.Property(e => e.Isdeleted)
                .HasDefaultValueSql("false")
                .HasColumnName("isdeleted");
            entity.Property(e => e.Isoccupied)
                .HasDefaultValueSql("false")
                .HasColumnName("isoccupied");
            entity.Property(e => e.Isrunning).HasColumnName("isrunning");
            entity.Property(e => e.Sectionid).HasColumnName("sectionid");
            entity.Property(e => e.Tablecapacity)
                .HasMaxLength(100)
                .HasColumnName("tablecapacity");
            entity.Property(e => e.Tablename)
                .HasMaxLength(20)
                .HasColumnName("tablename");

            entity.HasOne(d => d.Section).WithMany(p => p.Tables)
                .HasForeignKey(d => d.Sectionid)
                .HasConstraintName("tables_sectionid_fkey");
        });

        modelBuilder.Entity<Taxis>(entity =>
        {
            entity.HasKey(e => e.Taxesid).HasName("taxes_pkey");

            entity.ToTable("taxes");

            entity.Property(e => e.Taxesid).HasColumnName("taxesid");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.EditDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("edit_date");
            entity.Property(e => e.EditedBy)
                .HasMaxLength(50)
                .HasColumnName("edited_by");
            entity.Property(e => e.Isdefault)
                .HasDefaultValueSql("true")
                .HasColumnName("isdefault");
            entity.Property(e => e.Isdeleted)
                .HasDefaultValueSql("false")
                .HasColumnName("isdeleted");
            entity.Property(e => e.Isenabled)
                .HasDefaultValueSql("false")
                .HasColumnName("isenabled");
            entity.Property(e => e.Taxname)
                .HasMaxLength(20)
                .HasColumnName("taxname");
            entity.Property(e => e.Taxtype)
                .HasMaxLength(20)
                .HasColumnName("taxtype");
            entity.Property(e => e.Taxvalue).HasColumnName("taxvalue");
        });

        modelBuilder.Entity<Totalrating>(entity =>
        {
            entity.HasKey(e => e.Ratingid).HasName("totalrating_pkey");

            entity.ToTable("totalrating");

            entity.Property(e => e.Ratingid).HasColumnName("ratingid");
            entity.Property(e => e.Ambiancerating).HasColumnName("ambiancerating");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.Foodrating).HasColumnName("foodrating");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Servicerating).HasColumnName("servicerating");

            entity.HasOne(d => d.Order).WithMany(p => p.Totalratings)
                .HasForeignKey(d => d.Orderid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("totalrating_orderid_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Cityid).HasColumnName("cityid");
            entity.Property(e => e.Countryid).HasColumnName("countryid");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.EditDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("edit_date");
            entity.Property(e => e.EditedBy)
                .HasMaxLength(50)
                .HasColumnName("edited_by");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.Firstname)
                .HasMaxLength(255)
                .HasColumnName("firstname");
            entity.Property(e => e.Isactive)
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.Isdeleted)
                .HasDefaultValueSql("false")
                .HasColumnName("isdeleted");
            entity.Property(e => e.Lastname)
                .HasMaxLength(255)
                .HasColumnName("lastname");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(10)
                .HasColumnName("phone");
            entity.Property(e => e.Profilepic)
                .HasMaxLength(500)
                .HasColumnName("profilepic");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Stateid).HasColumnName("stateid");
            entity.Property(e => e.Username)
                .HasMaxLength(20)
                .HasColumnName("username");
            entity.Property(e => e.Zipcode).HasColumnName("zipcode");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.Roleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_roleid_fkey");
        });

        modelBuilder.Entity<WaitingTable>(entity =>
        {
            entity.HasKey(e => e.Waitingid).HasName("waiting_table_pkey");

            entity.ToTable("waiting_table");

            entity.Property(e => e.Waitingid).HasColumnName("waitingid");
            entity.Property(e => e.Assigntime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("assigntime");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Customerid).HasColumnName("customerid");
            entity.Property(e => e.EditDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("edit_date");
            entity.Property(e => e.EditedBy)
                .HasMaxLength(50)
                .HasColumnName("edited_by");
            entity.Property(e => e.Isassigned)
                .HasDefaultValueSql("false")
                .HasColumnName("isassigned");
            entity.Property(e => e.Isdeleted)
                .HasDefaultValueSql("false")
                .HasColumnName("isdeleted");
            entity.Property(e => e.Sectionid).HasColumnName("sectionid");
            entity.Property(e => e.Tokendate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("tokendate");
            entity.Property(e => e.Tokennumber).HasColumnName("tokennumber");
            entity.Property(e => e.Totalperson).HasColumnName("totalperson");

            entity.HasOne(d => d.Customer).WithMany(p => p.WaitingTables)
                .HasForeignKey(d => d.Customerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("waiting_table_customerid_fkey");

            entity.HasOne(d => d.Section).WithMany(p => p.WaitingTables)
                .HasForeignKey(d => d.Sectionid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("waiting_table_sectionid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
