﻿// <auto-generated />
using System;
using Chat.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Chat.Api.Migrations
{
    [DbContext(typeof(ChatDbContext))]
    [Migration("20250318094635_AddEvents")]
    partial class AddEvents
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("chat")
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Chat.Api.Message", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("Timestamp")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("Message", "chat");
                });

            modelBuilder.Entity("Chat.Messaging.Ef.EventLog", b =>
                {
                    b.Property<long>("LocalOffset")
                        .HasColumnType("bigint");

                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<byte[]>("PartitionKey")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<byte[]>("Payload")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<long>("Timestamp")
                        .HasColumnType("bigint");

                    b.Property<string>("Topic")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LocalOffset");

                    b.HasIndex("Topic");

                    b.HasIndex("Type");

                    b.HasIndex("Type", "PartitionKey");

                    b.ToTable("EventLog", "chat");
                });

            modelBuilder.Entity("Chat.Messaging.Ef.EventOutbox", b =>
                {
                    b.Property<long>("LocalOffset")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("LocalOffset"));

                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<byte[]>("PartitionKey")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<byte[]>("Payload")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<long>("Timestamp")
                        .HasColumnType("bigint");

                    b.Property<string>("Topic")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LocalOffset");

                    b.ToTable("EventOutbox", "chat");
                });

            modelBuilder.Entity("Chat.Api.Message", b =>
                {
                    b.OwnsOne("Chat.Api.User", "Sender", b1 =>
                        {
                            b1.Property<Guid>("MessageId")
                                .HasColumnType("uuid");

                            b1.Property<string>("Email")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.HasKey("MessageId");

                            b1.ToTable("Message", "chat");

                            b1.WithOwner()
                                .HasForeignKey("MessageId");
                        });

                    b.Navigation("Sender")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
