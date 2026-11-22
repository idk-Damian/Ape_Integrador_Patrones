using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjustesDominioFase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auditorias_Usuarios_UsuarioId",
                table: "Auditorias");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosInventario_Pedidos_PedidoId",
                table: "MovimientosInventario");

            migrationBuilder.DropIndex(
                name: "IX_PedidoDetalles_PedidoId",
                table: "PedidoDetalles");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_PedidoId",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_KitComponentes_KitPadreId",
                table: "KitComponentes");

            migrationBuilder.DropIndex(
                name: "IX_EventosOutbox_Procesado",
                table: "EventosOutbox");

            migrationBuilder.DropIndex(
                name: "IX_Despachos_PedidoId",
                table: "Despachos");

            migrationBuilder.DropIndex(
                name: "IX_Auditorias_Entidad_EntidadId",
                table: "Auditorias");

            migrationBuilder.DropIndex(
                name: "IX_Auditorias_UsuarioId",
                table: "Auditorias");

            migrationBuilder.DropColumn(
                name: "CodigoComponente",
                table: "KitComponentes");

            migrationBuilder.DropColumn(
                name: "TipoComponente",
                table: "KitComponentes");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Despachos");

            migrationBuilder.DropColumn(
                name: "EntidadId",
                table: "Auditorias");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Auditorias");

            migrationBuilder.RenameColumn(
                name: "Procesado",
                table: "EventosOutbox",
                newName: "Intentos");

            migrationBuilder.RenameColumn(
                name: "Monto",
                table: "Devoluciones",
                newName: "MontoReembolso");

            migrationBuilder.RenameColumn(
                name: "Fecha",
                table: "Despachos",
                newName: "FechaEstimada");

            migrationBuilder.RenameColumn(
                name: "Entidad",
                table: "Auditorias",
                newName: "Usuario");

            migrationBuilder.AlterColumn<string>(
                name: "Rol",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<decimal>(
                name: "PesoKg",
                table: "Productos",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Productos",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Productos",
                type: "TEXT",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "CategoriaCliente",
                table: "Pedidos",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEntrega",
                table: "Pedidos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Pedidos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Orden",
                table: "PedidoDetalles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "PesoKg",
                table: "PedidoDetalleComponentes",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AddColumn<int>(
                name: "Orden",
                table: "PedidoDetalleComponentes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Proveedor",
                table: "Pagos",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "MovimientosInventario",
                type: "TEXT",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "Motivo",
                table: "MovimientosInventario",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Referencia",
                table: "MovimientosInventario",
                type: "TEXT",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "StockResultante",
                table: "MovimientosInventario",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Orden",
                table: "KitComponentes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "KitComponentes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubKitId",
                table: "KitComponentes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaProcesado",
                table: "EventosOutbox",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdEvento",
                table: "EventosOutbox",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "UltimoError",
                table: "EventosOutbox",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiasDesdeEntrega",
                table: "Devoluciones",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ProductoSinUso",
                table: "Devoluciones",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReferenciaReembolso",
                table: "Devoluciones",
                type: "TEXT",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaDespacho",
                table: "Despachos",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "NumeroGuia",
                table: "Despachos",
                type: "TEXT",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoEntrega",
                table: "Despachos",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Categoria",
                table: "Clientes",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Clientes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Clientes",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Auditorias",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Correlacion",
                table: "Auditorias",
                type: "TEXT",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Referencia",
                table: "Auditorias",
                type: "TEXT",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PedidoHistorialEstados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PedidoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Secuencia = table.Column<int>(type: "INTEGER", nullable: false),
                    EstadoAnterior = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    EstadoNuevo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoHistorialEstados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidoHistorialEstados_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Nombre",
                table: "Productos",
                column: "Nombre");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Productos_Peso",
                table: "Productos",
                sql: "\"PesoKg\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Productos_Precio",
                table: "Productos",
                sql: "\"PrecioUnitario\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Productos_Stock",
                table: "Productos",
                sql: "\"Stock\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_Estado",
                table: "Pedidos",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoDetalles_PedidoId_ElementoCodigo",
                table: "PedidoDetalles",
                columns: new[] { "PedidoId", "ElementoCodigo" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PedidoDetalles_Cantidad",
                table: "PedidoDetalles",
                sql: "\"Cantidad\" > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_PedidoId",
                table: "Pagos",
                column: "PedidoId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Pagos_Monto",
                table: "Pagos",
                sql: "\"Monto\" > 0");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_PedidoId_ProductoId_Tipo",
                table: "MovimientosInventario",
                columns: new[] { "PedidoId", "ProductoId", "Tipo" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Movimientos_Cantidad",
                table: "MovimientosInventario",
                sql: "\"Cantidad\" <> 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Movimientos_StockResultante",
                table: "MovimientosInventario",
                sql: "\"StockResultante\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_KitComponentes_KitPadreId_ProductoId",
                table: "KitComponentes",
                columns: new[] { "KitPadreId", "ProductoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KitComponentes_KitPadreId_SubKitId",
                table: "KitComponentes",
                columns: new[] { "KitPadreId", "SubKitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KitComponentes_ProductoId",
                table: "KitComponentes",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_KitComponentes_SubKitId",
                table: "KitComponentes",
                column: "SubKitId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_KitComponentes_Cantidad",
                table: "KitComponentes",
                sql: "\"Cantidad\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_KitComponentes_Destino",
                table: "KitComponentes",
                sql: "(\"ProductoId\" IS NULL AND \"SubKitId\" IS NOT NULL) OR (\"ProductoId\" IS NOT NULL AND \"SubKitId\" IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_EventosOutbox_FechaProcesado",
                table: "EventosOutbox",
                column: "FechaProcesado");

            migrationBuilder.CreateIndex(
                name: "IX_EventosOutbox_IdEvento",
                table: "EventosOutbox",
                column: "IdEvento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devoluciones_ReferenciaReembolso",
                table: "Devoluciones",
                column: "ReferenciaReembolso",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Devoluciones_Monto",
                table: "Devoluciones",
                sql: "\"MontoReembolso\" > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Despachos_NumeroGuia",
                table: "Despachos",
                column: "NumeroGuia",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Despachos_PedidoId",
                table: "Despachos",
                column: "PedidoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Nombre",
                table: "Clientes",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_Categoria",
                table: "Auditorias",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_Fecha",
                table: "Auditorias",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoHistorialEstados_PedidoId_Secuencia",
                table: "PedidoHistorialEstados",
                columns: new[] { "PedidoId", "Secuencia" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_KitComponentes_Kits_SubKitId",
                table: "KitComponentes",
                column: "SubKitId",
                principalTable: "Kits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KitComponentes_Productos_ProductoId",
                table: "KitComponentes",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosInventario_Pedidos_PedidoId",
                table: "MovimientosInventario",
                column: "PedidoId",
                principalTable: "Pedidos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KitComponentes_Kits_SubKitId",
                table: "KitComponentes");

            migrationBuilder.DropForeignKey(
                name: "FK_KitComponentes_Productos_ProductoId",
                table: "KitComponentes");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosInventario_Pedidos_PedidoId",
                table: "MovimientosInventario");

            migrationBuilder.DropTable(
                name: "PedidoHistorialEstados");

            migrationBuilder.DropIndex(
                name: "IX_Productos_Nombre",
                table: "Productos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Productos_Peso",
                table: "Productos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Productos_Precio",
                table: "Productos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Productos_Stock",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_Estado",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_PedidoDetalles_PedidoId_ElementoCodigo",
                table: "PedidoDetalles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PedidoDetalles_Cantidad",
                table: "PedidoDetalles");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_PedidoId",
                table: "Pagos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Pagos_Monto",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosInventario_PedidoId_ProductoId_Tipo",
                table: "MovimientosInventario");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Movimientos_Cantidad",
                table: "MovimientosInventario");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Movimientos_StockResultante",
                table: "MovimientosInventario");

            migrationBuilder.DropIndex(
                name: "IX_KitComponentes_KitPadreId_ProductoId",
                table: "KitComponentes");

            migrationBuilder.DropIndex(
                name: "IX_KitComponentes_KitPadreId_SubKitId",
                table: "KitComponentes");

            migrationBuilder.DropIndex(
                name: "IX_KitComponentes_ProductoId",
                table: "KitComponentes");

            migrationBuilder.DropIndex(
                name: "IX_KitComponentes_SubKitId",
                table: "KitComponentes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_KitComponentes_Cantidad",
                table: "KitComponentes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_KitComponentes_Destino",
                table: "KitComponentes");

            migrationBuilder.DropIndex(
                name: "IX_EventosOutbox_FechaProcesado",
                table: "EventosOutbox");

            migrationBuilder.DropIndex(
                name: "IX_EventosOutbox_IdEvento",
                table: "EventosOutbox");

            migrationBuilder.DropIndex(
                name: "IX_Devoluciones_ReferenciaReembolso",
                table: "Devoluciones");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Devoluciones_Monto",
                table: "Devoluciones");

            migrationBuilder.DropIndex(
                name: "IX_Despachos_NumeroGuia",
                table: "Despachos");

            migrationBuilder.DropIndex(
                name: "IX_Despachos_PedidoId",
                table: "Despachos");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Nombre",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Auditorias_Categoria",
                table: "Auditorias");

            migrationBuilder.DropIndex(
                name: "IX_Auditorias_Fecha",
                table: "Auditorias");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "FechaEntrega",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "Orden",
                table: "PedidoDetalles");

            migrationBuilder.DropColumn(
                name: "Orden",
                table: "PedidoDetalleComponentes");

            migrationBuilder.DropColumn(
                name: "Proveedor",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "Motivo",
                table: "MovimientosInventario");

            migrationBuilder.DropColumn(
                name: "Referencia",
                table: "MovimientosInventario");

            migrationBuilder.DropColumn(
                name: "StockResultante",
                table: "MovimientosInventario");

            migrationBuilder.DropColumn(
                name: "Orden",
                table: "KitComponentes");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "KitComponentes");

            migrationBuilder.DropColumn(
                name: "SubKitId",
                table: "KitComponentes");

            migrationBuilder.DropColumn(
                name: "FechaProcesado",
                table: "EventosOutbox");

            migrationBuilder.DropColumn(
                name: "IdEvento",
                table: "EventosOutbox");

            migrationBuilder.DropColumn(
                name: "UltimoError",
                table: "EventosOutbox");

            migrationBuilder.DropColumn(
                name: "DiasDesdeEntrega",
                table: "Devoluciones");

            migrationBuilder.DropColumn(
                name: "ProductoSinUso",
                table: "Devoluciones");

            migrationBuilder.DropColumn(
                name: "ReferenciaReembolso",
                table: "Devoluciones");

            migrationBuilder.DropColumn(
                name: "FechaDespacho",
                table: "Despachos");

            migrationBuilder.DropColumn(
                name: "NumeroGuia",
                table: "Despachos");

            migrationBuilder.DropColumn(
                name: "TipoEntrega",
                table: "Despachos");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Auditorias");

            migrationBuilder.DropColumn(
                name: "Correlacion",
                table: "Auditorias");

            migrationBuilder.DropColumn(
                name: "Referencia",
                table: "Auditorias");

            migrationBuilder.RenameColumn(
                name: "Intentos",
                table: "EventosOutbox",
                newName: "Procesado");

            migrationBuilder.RenameColumn(
                name: "MontoReembolso",
                table: "Devoluciones",
                newName: "Monto");

            migrationBuilder.RenameColumn(
                name: "FechaEstimada",
                table: "Despachos",
                newName: "Fecha");

            migrationBuilder.RenameColumn(
                name: "Usuario",
                table: "Auditorias",
                newName: "Entidad");

            migrationBuilder.AlterColumn<int>(
                name: "Rol",
                table: "Usuarios",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<decimal>(
                name: "PesoKg",
                table: "Productos",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AlterColumn<int>(
                name: "CategoriaCliente",
                table: "Pedidos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<decimal>(
                name: "PesoKg",
                table: "PedidoDetalleComponentes",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AlterColumn<int>(
                name: "Tipo",
                table: "MovimientosInventario",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<string>(
                name: "CodigoComponente",
                table: "KitComponentes",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoComponente",
                table: "KitComponentes",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Despachos",
                type: "TEXT",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Categoria",
                table: "Clientes",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<int>(
                name: "EntidadId",
                table: "Auditorias",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Auditorias",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PedidoDetalles_PedidoId",
                table: "PedidoDetalles",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_PedidoId",
                table: "Pagos",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_KitComponentes_KitPadreId",
                table: "KitComponentes",
                column: "KitPadreId");

            migrationBuilder.CreateIndex(
                name: "IX_EventosOutbox_Procesado",
                table: "EventosOutbox",
                column: "Procesado");

            migrationBuilder.CreateIndex(
                name: "IX_Despachos_PedidoId",
                table: "Despachos",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_Entidad_EntidadId",
                table: "Auditorias",
                columns: new[] { "Entidad", "EntidadId" });

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_UsuarioId",
                table: "Auditorias",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auditorias_Usuarios_UsuarioId",
                table: "Auditorias",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosInventario_Pedidos_PedidoId",
                table: "MovimientosInventario",
                column: "PedidoId",
                principalTable: "Pedidos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
