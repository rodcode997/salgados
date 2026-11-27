IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE TABLE [Configuracoes] (
        [Id] int NOT NULL IDENTITY,
        [ExigirAceiteDivida] bit NOT NULL,
        [TipoDividaAniversario] int NOT NULL,
        [ValorMinimoQuitacaoConjunta] decimal(10,2) NOT NULL,
        CONSTRAINT [PK_Configuracoes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE TABLE [Motivos] (
        [Id] int NOT NULL IDENTITY,
        [Descricao] nvarchar(200) NOT NULL,
        [Ativo] bit NOT NULL,
        CONSTRAINT [PK_Motivos] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE TABLE [Perfis] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(100) NOT NULL,
        [Descricao] nvarchar(300) NULL,
        [Ativo] bit NOT NULL,
        CONSTRAINT [PK_Perfis] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE TABLE [Permissoes] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(80) NOT NULL,
        [Descricao] nvarchar(200) NOT NULL,
        [Modulo] nvarchar(60) NOT NULL,
        CONSTRAINT [PK_Permissoes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE TABLE [Produtos] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(150) NOT NULL,
        [Valor] decimal(10,2) NOT NULL,
        [Ativo] bit NOT NULL,
        [CriadoEm] datetime2 NOT NULL,
        CONSTRAINT [PK_Produtos] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(150) NOT NULL,
        [Login] nvarchar(100) NOT NULL,
        [SenhaHash] nvarchar(max) NOT NULL,
        [DataNascimento] datetime2 NULL,
        [Avatar] nvarchar(500) NULL,
        [DeveAlterarSenha] bit NOT NULL,
        [Ativo] bit NOT NULL,
        [CriadoEm] datetime2 NOT NULL,
        [PerfilId] int NOT NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Usuarios_Perfis_PerfilId] FOREIGN KEY ([PerfilId]) REFERENCES [Perfis] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE TABLE [PerfilPermissoes] (
        [PerfilId] int NOT NULL,
        [PermissaoId] int NOT NULL,
        CONSTRAINT [PK_PerfilPermissoes] PRIMARY KEY ([PerfilId], [PermissaoId]),
        CONSTRAINT [FK_PerfilPermissoes_Perfis_PerfilId] FOREIGN KEY ([PerfilId]) REFERENCES [Perfis] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PerfilPermissoes_Permissoes_PermissaoId] FOREIGN KEY ([PermissaoId]) REFERENCES [Permissoes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE TABLE [Dividas] (
        [Id] int NOT NULL IDENTITY,
        [UsuarioId] int NOT NULL,
        [MotivoId] int NOT NULL,
        [Tipo] int NOT NULL,
        [Observacao] nvarchar(500) NULL,
        [DataAquisicao] datetime2 NOT NULL,
        [Status] int NOT NULL,
        [CriadoPorId] int NOT NULL,
        [CriadoEm] datetime2 NOT NULL,
        [DataQuitacao] datetime2 NULL,
        [ValorPago] decimal(10,2) NULL,
        [QuitadoPorId] int NULL,
        [QuitadoEm] datetime2 NULL,
        CONSTRAINT [PK_Dividas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Dividas_Motivos_MotivoId] FOREIGN KEY ([MotivoId]) REFERENCES [Motivos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Dividas_Usuarios_CriadoPorId] FOREIGN KEY ([CriadoPorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Dividas_Usuarios_QuitadoPorId] FOREIGN KEY ([QuitadoPorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Dividas_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE TABLE [GruposPagamento] (
        [Id] int NOT NULL IDENTITY,
        [ProdutoId] int NOT NULL,
        [ValorMeta] decimal(10,2) NOT NULL,
        [ValorMinimoPorDivida] decimal(10,2) NOT NULL,
        [DataCompra] datetime2 NULL,
        [Observacao] nvarchar(300) NULL,
        [Status] int NOT NULL,
        [CriadoPorId] int NOT NULL,
        [CriadoEm] datetime2 NOT NULL,
        [FechadoEm] datetime2 NULL,
        CONSTRAINT [PK_GruposPagamento] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GruposPagamento_Produtos_ProdutoId] FOREIGN KEY ([ProdutoId]) REFERENCES [Produtos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GruposPagamento_Usuarios_CriadoPorId] FOREIGN KEY ([CriadoPorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE TABLE [DividaInteracoes] (
        [Id] int NOT NULL IDENTITY,
        [DividaId] int NOT NULL,
        [UsuarioId] int NOT NULL,
        [Tipo] int NOT NULL,
        [Mensagem] nvarchar(600) NULL,
        [CriadoEm] datetime2 NOT NULL,
        CONSTRAINT [PK_DividaInteracoes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DividaInteracoes_Dividas_DividaId] FOREIGN KEY ([DividaId]) REFERENCES [Dividas] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DividaInteracoes_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE TABLE [Pagamentos] (
        [Id] int NOT NULL IDENTITY,
        [DividaId] int NOT NULL,
        [DataPagamento] datetime2 NOT NULL,
        [ValorPago] decimal(10,2) NOT NULL,
        [Observacao] nvarchar(500) NULL,
        [Status] int NOT NULL,
        [CriadoPorId] int NOT NULL,
        [CriadoEm] datetime2 NOT NULL,
        [AvaliadoPorId] int NULL,
        [AvaliadoEm] datetime2 NULL,
        [MotivoRecusa] nvarchar(500) NULL,
        CONSTRAINT [PK_Pagamentos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Pagamentos_Dividas_DividaId] FOREIGN KEY ([DividaId]) REFERENCES [Dividas] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Pagamentos_Usuarios_AvaliadoPorId] FOREIGN KEY ([AvaliadoPorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Pagamentos_Usuarios_CriadoPorId] FOREIGN KEY ([CriadoPorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE TABLE [GrupoPagamentoParticipacoes] (
        [Id] int NOT NULL IDENTITY,
        [GrupoPagamentoId] int NOT NULL,
        [UsuarioId] int NOT NULL,
        [ValorContribuido] decimal(10,2) NOT NULL,
        [CriadoEm] datetime2 NOT NULL,
        CONSTRAINT [PK_GrupoPagamentoParticipacoes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GrupoPagamentoParticipacoes_GruposPagamento_GrupoPagamentoId] FOREIGN KEY ([GrupoPagamentoId]) REFERENCES [GruposPagamento] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_GrupoPagamentoParticipacoes_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE TABLE [GrupoPagamentoDividas] (
        [GrupoPagamentoParticipacaoId] int NOT NULL,
        [DividaId] int NOT NULL,
        CONSTRAINT [PK_GrupoPagamentoDividas] PRIMARY KEY ([GrupoPagamentoParticipacaoId], [DividaId]),
        CONSTRAINT [FK_GrupoPagamentoDividas_Dividas_DividaId] FOREIGN KEY ([DividaId]) REFERENCES [Dividas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GrupoPagamentoDividas_GrupoPagamentoParticipacoes_GrupoPagamentoParticipacaoId] FOREIGN KEY ([GrupoPagamentoParticipacaoId]) REFERENCES [GrupoPagamentoParticipacoes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ExigirAceiteDivida', N'TipoDividaAniversario', N'ValorMinimoQuitacaoConjunta') AND [object_id] = OBJECT_ID(N'[Configuracoes]'))
        SET IDENTITY_INSERT [Configuracoes] ON;
    EXEC(N'INSERT INTO [Configuracoes] ([Id], [ExigirAceiteDivida], [TipoDividaAniversario], [ValorMinimoQuitacaoConjunta])
    VALUES (1, CAST(1 AS bit), 1, 30.0)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ExigirAceiteDivida', N'TipoDividaAniversario', N'ValorMinimoQuitacaoConjunta') AND [object_id] = OBJECT_ID(N'[Configuracoes]'))
        SET IDENTITY_INSERT [Configuracoes] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Ativo', N'Descricao') AND [object_id] = OBJECT_ID(N'[Motivos]'))
        SET IDENTITY_INSERT [Motivos] ON;
    EXEC(N'INSERT INTO [Motivos] ([Id], [Ativo], [Descricao])
    VALUES (1, CAST(1 AS bit), N''Aniversário''),
    (2, CAST(1 AS bit), N''Atraso em reunião''),
    (3, CAST(1 AS bit), N''Quebrou o build''),
    (4, CAST(1 AS bit), N''Bug em produção''),
    (5, CAST(1 AS bit), N''Esqueceu de bloquear o computador'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Ativo', N'Descricao') AND [object_id] = OBJECT_ID(N'[Motivos]'))
        SET IDENTITY_INSERT [Motivos] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Ativo', N'Descricao', N'Nome') AND [object_id] = OBJECT_ID(N'[Perfis]'))
        SET IDENTITY_INSERT [Perfis] ON;
    EXEC(N'INSERT INTO [Perfis] ([Id], [Ativo], [Descricao], [Nome])
    VALUES (1, CAST(1 AS bit), N''Acesso total ao sistema, usuários, perfis e aprovação de pagamentos'', N''Administrador''),
    (2, CAST(1 AS bit), N''Gerencia dívidas, pagamentos e motivos'', N''Gestor''),
    (3, CAST(1 AS bit), N''Registra dívidas, aceita/contesta e informa pagamentos'', N''Colaborador'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Ativo', N'Descricao', N'Nome') AND [object_id] = OBJECT_ID(N'[Perfis]'))
        SET IDENTITY_INSERT [Perfis] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Codigo', N'Descricao', N'Modulo') AND [object_id] = OBJECT_ID(N'[Permissoes]'))
        SET IDENTITY_INSERT [Permissoes] ON;
    EXEC(N'INSERT INTO [Permissoes] ([Id], [Codigo], [Descricao], [Modulo])
    VALUES (1, N''dividas.ver'', N''Visualizar dívidas e painel'', N''Dívidas''),
    (2, N''dividas.criar'', N''Registrar novas dívidas'', N''Dívidas''),
    (3, N''dividas.quitar'', N''Aprovar/recusar ou cadastrar pagamento aprovado'', N''Dívidas''),
    (4, N''usuarios.ver'', N''Visualizar equipe e histórico de dívidas'', N''Usuários''),
    (5, N''usuarios.gerenciar'', N''Cadastrar, editar e ativar/desativar usuários'', N''Usuários''),
    (6, N''motivos.ver'', N''Visualizar motivos'', N''Motivos''),
    (7, N''motivos.gerenciar'', N''Cadastrar e ativar/desativar motivos'', N''Motivos''),
    (8, N''perfis.gerenciar'', N''Gerenciar perfis e permissões'', N''Segurança'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Codigo', N'Descricao', N'Modulo') AND [object_id] = OBJECT_ID(N'[Permissoes]'))
        SET IDENTITY_INSERT [Permissoes] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PerfilId', N'PermissaoId') AND [object_id] = OBJECT_ID(N'[PerfilPermissoes]'))
        SET IDENTITY_INSERT [PerfilPermissoes] ON;
    EXEC(N'INSERT INTO [PerfilPermissoes] ([PerfilId], [PermissaoId])
    VALUES (1, 1),
    (1, 2),
    (1, 3),
    (1, 4),
    (1, 5),
    (1, 6),
    (1, 7),
    (1, 8),
    (2, 1),
    (2, 2),
    (2, 3),
    (2, 4),
    (2, 6),
    (2, 7),
    (3, 1),
    (3, 2),
    (3, 4),
    (3, 6)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PerfilId', N'PermissaoId') AND [object_id] = OBJECT_ID(N'[PerfilPermissoes]'))
        SET IDENTITY_INSERT [PerfilPermissoes] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DividaInteracoes_DividaId] ON [DividaInteracoes] ([DividaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DividaInteracoes_UsuarioId] ON [DividaInteracoes] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Dividas_CriadoPorId] ON [Dividas] ([CriadoPorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Dividas_MotivoId] ON [Dividas] ([MotivoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Dividas_QuitadoPorId] ON [Dividas] ([QuitadoPorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Dividas_UsuarioId] ON [Dividas] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GrupoPagamentoDividas_DividaId] ON [GrupoPagamentoDividas] ([DividaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GrupoPagamentoParticipacoes_GrupoPagamentoId] ON [GrupoPagamentoParticipacoes] ([GrupoPagamentoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GrupoPagamentoParticipacoes_UsuarioId] ON [GrupoPagamentoParticipacoes] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GruposPagamento_CriadoPorId] ON [GruposPagamento] ([CriadoPorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GruposPagamento_ProdutoId] ON [GruposPagamento] ([ProdutoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Pagamentos_AvaliadoPorId] ON [Pagamentos] ([AvaliadoPorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Pagamentos_CriadoPorId] ON [Pagamentos] ([CriadoPorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Pagamentos_DividaId] ON [Pagamentos] ([DividaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PerfilPermissoes_PermissaoId] ON [PerfilPermissoes] ([PermissaoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Perfis_Nome] ON [Perfis] ([Nome]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Permissoes_Codigo] ON [Permissoes] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Login] ON [Usuarios] ([Login]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Usuarios_PerfilId] ON [Usuarios] ([PerfilId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127023036_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261127023036_InitialCreate', N'10.0.15');
END;

COMMIT;
GO

