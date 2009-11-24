DROP TABLE IF EXISTS "Account";
CREATE TABLE [Account] (
    [Id] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
    [Name] text NOT NULL COLLATE NOCASE UNIQUE,
    [Password] text NOT NULL
);
DROP TABLE IF EXISTS "Player";
CREATE TABLE [Player] (
    [Id] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
    [AccountId] integer NOT NULL,
    [Name] text NOT NULL COLLATE NOCASE UNIQUE,
    [Gender] integer NOT NULL,
    [Vocation] integer NOT NULL DEFAULT 0,
    [Level] integer NOT NULL DEFAULT 1,
    [MagicLevel] integer NOT NULL DEFAULT 0,
    [Experience] integer NOT NULL DEFAULT 0,
    [MaxHealth] integer NOT NULL DEFAULT 100,
    [MaxMana] integer NOT NULL DEFAULT 100,
    [Capacity] integer NOT NULL DEFAULT 0,
    [OutfitLookType] integer NOT NULL DEFAULT 128,
    [OutfitHead] integer NOT NULL DEFAULT 0,
    [OutfitBody] integer NOT NULL DEFAULT 0,
    [OutfitLegs] integer NOT NULL DEFAULT 0,
    [OutfitFeet] integer NOT NULL DEFAULT 0,
    [OutfitAddons] integer NOT NULL DEFAULT 0,
    [LocationX] integer,
    [LocationY] integer,
    [LocationZ] integer,
    [Direction] integer,
    [LastLogin] integer NOT NULL DEFAULT 0,
    CONSTRAINT [FK_Player_0] FOREIGN KEY ([AccountId]) REFERENCES [Account] ([Id])
);
DROP TABLE IF EXISTS "PlayerInventory";
CREATE TABLE [PlayerInventory] (
    [PlayerId] integer NOT NULL,
    [Slot] integer NOT NULL,
    [ItemId] integer NOT NULL,
    CONSTRAINT [PK_PlayerInventory] PRIMARY KEY ([PlayerId], [Slot]),
    CONSTRAINT [FK_PlayerInventory_0] FOREIGN KEY ([PlayerId]) REFERENCES [Player] ([Id]),
    CONSTRAINT [FK_PlayerInventory_1] FOREIGN KEY ([ItemId]) REFERENCES [Item] ([Id])
);
DROP TABLE IF EXISTS "Item";
CREATE TABLE [Item] (
    [Id] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
    [SpriteId] integer NOT NULL,
    [Extra] integer NOT NULL,
    [ParentItemId] integer,
    CONSTRAINT [FK_Item_1] FOREIGN KEY ([ParentItemId]) REFERENCES [Item] ([Id])
);