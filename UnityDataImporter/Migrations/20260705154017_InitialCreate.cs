using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UnityDataImporter.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EffectType",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EffectType", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentSlotType",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentSlotType", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "eventType",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    icon = table.Column<byte[]>(type: "bytea", nullable: true),
                    eventName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventType", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "HoldType",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoldType", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "InventorySound",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sound = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventorySound", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "itemAudio",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    audio = table.Column<byte[]>(type: "bytea", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    prefix = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itemAudio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ItemType",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemType", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "LootTables",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lootTableDatas = table.Column<string>(type: "jsonb", nullable: false),
                    lootTableName = table.Column<string>(type: "text", nullable: false),
                    lootTableId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LootTables", x => x.id);
                    table.UniqueConstraint("AK_LootTables_lootTableId", x => x.lootTableId);
                });

            migrationBuilder.CreateTable(
                name: "magicStatusEffect",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_magicStatusEffect", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "magicType",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_magicType", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Rarity",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rarity", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    recipeName = table.Column<string>(type: "text", nullable: false),
                    inputItems = table.Column<string>(type: "json", nullable: true),
                    outputItems = table.Column<string>(type: "json", nullable: true),
                    recipeCost = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Stats",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stats", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Vector2",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    x = table.Column<double>(type: "double precision", nullable: false),
                    y = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vector2", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "NpcsShop",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    recipes = table.Column<string>(type: "jsonb", nullable: false),
                    lootTableId = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NpcsShop", x => x.id);
                    table.ForeignKey(
                        name: "FK_NpcsShop_LootTables_lootTableId",
                        column: x => x.lootTableId,
                        principalTable: "LootTables",
                        principalColumn: "lootTableId");
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    isStackable = table.Column<bool>(type: "boolean", nullable: true),
                    maxAmount = table.Column<int>(type: "integer", nullable: true),
                    icon = table.Column<byte[]>(type: "bytea", nullable: true),
                    itemSize = table.Column<long>(type: "bigint", nullable: true),
                    buyAmount = table.Column<int>(type: "integer", nullable: true),
                    sellAmount = table.Column<int>(type: "integer", nullable: true),
                    canBlock = table.Column<bool>(type: "boolean", nullable: true),
                    blockAmount = table.Column<float>(type: "real", nullable: true),
                    blockSounds = table.Column<long>(type: "bigint", nullable: true),
                    parryAudio = table.Column<long>(type: "bigint", nullable: true),
                    itemRarity = table.Column<string>(type: "text", nullable: true),
                    itemType = table.Column<string>(type: "text", nullable: true),
                    itemStats = table.Column<string>(type: "jsonb", nullable: true),
                    itemEvents = table.Column<string>(type: "jsonb", nullable: true),
                    equipmentSlot = table.Column<string>(type: "text", nullable: true),
                    itemSoundType = table.Column<long>(type: "bigint", nullable: true),
                    holdType = table.Column<string>(type: "text", nullable: true),
                    itemAnimations = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_EquipmentSlotType_equipmentSlot",
                        column: x => x.equipmentSlot,
                        principalTable: "EquipmentSlotType",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Items_HoldType_holdType",
                        column: x => x.holdType,
                        principalTable: "HoldType",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Items_InventorySound_itemSoundType",
                        column: x => x.itemSoundType,
                        principalTable: "InventorySound",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Items_ItemType_itemType",
                        column: x => x.itemType,
                        principalTable: "ItemType",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Items_Rarity_itemRarity",
                        column: x => x.itemRarity,
                        principalTable: "Rarity",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Items_Vector2_itemSize",
                        column: x => x.itemSize,
                        principalTable: "Vector2",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Items_itemAudio_blockSounds",
                        column: x => x.blockSounds,
                        principalTable: "itemAudio",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Items_itemAudio_parryAudio",
                        column: x => x.parryAudio,
                        principalTable: "itemAudio",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "itemEvent",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    itemId = table.Column<string>(type: "text", nullable: false),
                    eventTypeId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itemEvent", x => x.id);
                    table.ForeignKey(
                        name: "FK_itemEvent_Items_itemId",
                        column: x => x.itemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_itemEvent_eventType_eventTypeId",
                        column: x => x.eventTypeId,
                        principalTable: "eventType",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "lootTableData",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    itemId = table.Column<string>(type: "text", nullable: false),
                    probability = table.Column<int>(type: "integer", nullable: true),
                    minAmount = table.Column<int>(type: "integer", nullable: true),
                    maxAmount = table.Column<int>(type: "integer", nullable: true),
                    lootTableId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lootTableData", x => x.id);
                    table.ForeignKey(
                        name: "FK_lootTableData_Items_itemId",
                        column: x => x.itemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lootTableData_LootTables_lootTableId",
                        column: x => x.lootTableId,
                        principalTable: "LootTables",
                        principalColumn: "lootTableId");
                });

            migrationBuilder.CreateTable(
                name: "magicAttacks",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    itemId = table.Column<string>(type: "text", nullable: true),
                    magicType = table.Column<string>(type: "text", nullable: false),
                    magicDamage = table.Column<int>(type: "integer", nullable: true),
                    cooldown = table.Column<float>(type: "real", nullable: true),
                    projectileSpeed = table.Column<float>(type: "real", nullable: true),
                    effectType = table.Column<string>(type: "text", nullable: true),
                    hitSounds = table.Column<string>(type: "jsonb", nullable: true),
                    manaConsumption = table.Column<int>(type: "integer", nullable: true),
                    maxCompanions = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_magicAttacks", x => x.id);
                    table.ForeignKey(
                        name: "FK_magicAttacks_EffectType_effectType",
                        column: x => x.effectType,
                        principalTable: "EffectType",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_magicAttacks_Items_itemId",
                        column: x => x.itemId,
                        principalTable: "Items",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_magicAttacks_magicType_magicType",
                        column: x => x.magicType,
                        principalTable: "magicType",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SoundsArray",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    itemId = table.Column<string>(type: "text", nullable: false),
                    sounds = table.Column<string>(type: "json", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoundsArray", x => x.id);
                    table.ForeignKey(
                        name: "FK_SoundsArray_Items_itemId",
                        column: x => x.itemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stats_Amount",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    stat = table.Column<string>(type: "text", nullable: false),
                    item = table.Column<string>(type: "text", nullable: true),
                    amount = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stats_Amount", x => x.id);
                    table.ForeignKey(
                        name: "FK_Stats_Amount_Items_item",
                        column: x => x.item,
                        principalTable: "Items",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Stats_Amount_Stats_stat",
                        column: x => x.stat,
                        principalTable: "Stats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeaponData",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    itemId = table.Column<string>(type: "text", nullable: true),
                    damage = table.Column<long>(type: "bigint", nullable: true),
                    heaviness = table.Column<float>(type: "real", nullable: true),
                    ammo = table.Column<string>(type: "text", nullable: true),
                    cooldown = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeaponData", x => x.id);
                    table.ForeignKey(
                        name: "FK_WeaponData_Items_ammo",
                        column: x => x.ammo,
                        principalTable: "Items",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WeaponData_Items_itemId",
                        column: x => x.itemId,
                        principalTable: "Items",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_itemEvent_eventTypeId",
                table: "itemEvent",
                column: "eventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_itemEvent_itemId",
                table: "itemEvent",
                column: "itemId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_blockSounds",
                table: "Items",
                column: "blockSounds");

            migrationBuilder.CreateIndex(
                name: "IX_Items_equipmentSlot",
                table: "Items",
                column: "equipmentSlot");

            migrationBuilder.CreateIndex(
                name: "IX_Items_holdType",
                table: "Items",
                column: "holdType");

            migrationBuilder.CreateIndex(
                name: "IX_Items_itemRarity",
                table: "Items",
                column: "itemRarity");

            migrationBuilder.CreateIndex(
                name: "IX_Items_itemSize",
                table: "Items",
                column: "itemSize");

            migrationBuilder.CreateIndex(
                name: "IX_Items_itemSoundType",
                table: "Items",
                column: "itemSoundType");

            migrationBuilder.CreateIndex(
                name: "IX_Items_itemType",
                table: "Items",
                column: "itemType");

            migrationBuilder.CreateIndex(
                name: "IX_Items_parryAudio",
                table: "Items",
                column: "parryAudio");

            migrationBuilder.CreateIndex(
                name: "IX_lootTableData_itemId",
                table: "lootTableData",
                column: "itemId");

            migrationBuilder.CreateIndex(
                name: "IX_lootTableData_lootTableId",
                table: "lootTableData",
                column: "lootTableId");

            migrationBuilder.CreateIndex(
                name: "IX_LootTables_lootTableId",
                table: "LootTables",
                column: "lootTableId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_magicAttacks_effectType",
                table: "magicAttacks",
                column: "effectType");

            migrationBuilder.CreateIndex(
                name: "IX_magicAttacks_itemId",
                table: "magicAttacks",
                column: "itemId");

            migrationBuilder.CreateIndex(
                name: "IX_magicAttacks_magicType",
                table: "magicAttacks",
                column: "magicType");

            migrationBuilder.CreateIndex(
                name: "IX_NpcsShop_lootTableId",
                table: "NpcsShop",
                column: "lootTableId");

            migrationBuilder.CreateIndex(
                name: "IX_SoundsArray_itemId",
                table: "SoundsArray",
                column: "itemId");

            migrationBuilder.CreateIndex(
                name: "IX_Stats_Amount_item",
                table: "Stats_Amount",
                column: "item");

            migrationBuilder.CreateIndex(
                name: "IX_Stats_Amount_stat",
                table: "Stats_Amount",
                column: "stat");

            migrationBuilder.CreateIndex(
                name: "IX_WeaponData_ammo",
                table: "WeaponData",
                column: "ammo");

            migrationBuilder.CreateIndex(
                name: "IX_WeaponData_itemId",
                table: "WeaponData",
                column: "itemId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "itemEvent");

            migrationBuilder.DropTable(
                name: "lootTableData");

            migrationBuilder.DropTable(
                name: "magicAttacks");

            migrationBuilder.DropTable(
                name: "magicStatusEffect");

            migrationBuilder.DropTable(
                name: "NpcsShop");

            migrationBuilder.DropTable(
                name: "Recipes");

            migrationBuilder.DropTable(
                name: "SoundsArray");

            migrationBuilder.DropTable(
                name: "Stats_Amount");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "WeaponData");

            migrationBuilder.DropTable(
                name: "eventType");

            migrationBuilder.DropTable(
                name: "EffectType");

            migrationBuilder.DropTable(
                name: "magicType");

            migrationBuilder.DropTable(
                name: "LootTables");

            migrationBuilder.DropTable(
                name: "Stats");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "EquipmentSlotType");

            migrationBuilder.DropTable(
                name: "HoldType");

            migrationBuilder.DropTable(
                name: "InventorySound");

            migrationBuilder.DropTable(
                name: "ItemType");

            migrationBuilder.DropTable(
                name: "Rarity");

            migrationBuilder.DropTable(
                name: "Vector2");

            migrationBuilder.DropTable(
                name: "itemAudio");
        }
    }
}
