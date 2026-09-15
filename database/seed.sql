/* Datos iniciales para Bodega Norte. Ejecutar después de schema.sql en la base BodegaNorte. */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

INSERT INTO categories (name)
SELECT v.name FROM (VALUES
  (N'Lácteos'), (N'Panadería'), (N'Abarrotes'), (N'Bebidas'),
  (N'Snacks'), (N'Enlatados'), (N'Limpieza')
) v(name)
WHERE NOT EXISTS (SELECT 1 FROM categories c WHERE c.name = v.name);

INSERT INTO suppliers (name, contact_name, phone)
SELECT v.name, v.contact_name, v.phone
FROM (VALUES
  (N'Distribuidora Central', N'María Paredes', N'987 654 321'),
  (N'Alimentos del Valle', N'Luis Mendoza', N'986 212 480'),
  (N'Comercial San Luis', N'Rosa Salazar', N'985 730 118'),
  (N'Bebidas Andinas', N'Carlos Ruiz', N'982 440 607'),
  (N'Granja Santa Rosa', N'Ana Torres', N'981 330 929')
) v(name, contact_name, phone)
WHERE NOT EXISTS (SELECT 1 FROM suppliers s WHERE s.name = v.name);

INSERT INTO products (code, name, category_id, supplier_id, unit, stock, min_stock, cost, sale_price)
SELECT v.code, v.name, c.category_id, s.supplier_id, v.unit, v.stock, v.min_stock, v.cost, v.sale_price
FROM (VALUES
  (N'P-001', N'Leche Gloria 1L', N'Lácteos', N'Distribuidora Central', N'unidad', 3.00, 10.00, 3.25, 4.50),
  (N'P-002', N'Pan de molde Bimbo', N'Panadería', N'Alimentos del Valle', N'unidad', 5.00, 10.00, 6.20, 8.50),
  (N'P-003', N'Aceite Primor 1L', N'Abarrotes', N'Distribuidora Central', N'unidad', 4.00, 8.00, 7.80, 10.50),
  (N'P-004', N'Azúcar Rubia 1kg', N'Abarrotes', N'Comercial San Luis', N'unidad', 6.00, 10.00, 3.10, 4.20),
  (N'P-005', N'Arroz Costeño 5kg', N'Abarrotes', N'Comercial San Luis', N'unidad', 2.00, 8.00, 17.50, 22.90),
  (N'P-006', N'Gaseosa Inca Kola 1.5L', N'Bebidas', N'Bebidas Andinas', N'unidad', 24.00, 12.00, 5.80, 8.50),
  (N'P-007', N'Agua San Luis 625ml', N'Bebidas', N'Bebidas Andinas', N'unidad', 32.00, 15.00, 1.50, 2.50),
  (N'P-008', N'Galletas Casino Chocolate', N'Snacks', N'Alimentos del Valle', N'paquete', 18.00, 10.00, 1.20, 2.00),
  (N'P-009', N'Atún Florida 170g', N'Enlatados', N'Distribuidora Central', N'lata', 11.00, 6.00, 4.20, 6.20),
  (N'P-010', N'Huevos Pardos x 15', N'Lácteos', N'Granja Santa Rosa', N'bandeja', 9.00, 6.00, 8.80, 11.90),
  (N'P-011', N'Detergente Bolívar 500g', N'Limpieza', N'Comercial San Luis', N'unidad', 14.00, 8.00, 4.40, 6.30),
  (N'P-012', N'Papel higiénico Elite x 4', N'Limpieza', N'Alimentos del Valle', N'paquete', 7.00, 6.00, 7.10, 9.90)
) v(code, name, category, supplier, unit, stock, min_stock, cost, sale_price)
INNER JOIN categories c ON c.name = v.category
INNER JOIN suppliers s ON s.name = v.supplier
WHERE NOT EXISTS (SELECT 1 FROM products p WHERE p.code = v.code);

IF NOT EXISTS (SELECT 1 FROM sales)
BEGIN
  DECLARE @saleId INT;
  DECLARE @productId INT;

  INSERT INTO sales (sale_number, payment_method, total, sold_at) VALUES (N'#1048', N'Yape', 37.50, DATEADD(MINUTE, -30, SYSUTCDATETIME()));
  SET @saleId = SCOPE_IDENTITY();
  SELECT @productId = product_id FROM products WHERE code = N'P-001';
  INSERT INTO sale_items (sale_id, product_id, quantity, unit_price) VALUES (@saleId, @productId, 2, 4.50);
  SELECT @productId = product_id FROM products WHERE code = N'P-006';
  INSERT INTO sale_items (sale_id, product_id, quantity, unit_price) VALUES (@saleId, @productId, 2, 8.50);
  SELECT @productId = product_id FROM products WHERE code = N'P-008';
  INSERT INTO sale_items (sale_id, product_id, quantity, unit_price) VALUES (@saleId, @productId, 3, 2.00);

  INSERT INTO sales (sale_number, payment_method, total, sold_at) VALUES (N'#1047', N'Efectivo', 62.30, DATEADD(MINUTE, -58, SYSUTCDATETIME()));
  SET @saleId = SCOPE_IDENTITY();
  SELECT @productId = product_id FROM products WHERE code = N'P-002';
  INSERT INTO sale_items (sale_id, product_id, quantity, unit_price) VALUES (@saleId, @productId, 2, 8.50);
  SELECT @productId = product_id FROM products WHERE code = N'P-003';
  INSERT INTO sale_items (sale_id, product_id, quantity, unit_price) VALUES (@saleId, @productId, 1, 10.50);
  SELECT @productId = product_id FROM products WHERE code = N'P-009';
  INSERT INTO sale_items (sale_id, product_id, quantity, unit_price) VALUES (@saleId, @productId, 3, 6.20);

  INSERT INTO sales (sale_number, payment_method, total, sold_at) VALUES (N'#1046', N'Efectivo', 18.00, DATEADD(MINUTE, -102, SYSUTCDATETIME()));
  SET @saleId = SCOPE_IDENTITY();
  SELECT @productId = product_id FROM products WHERE code = N'P-006';
  INSERT INTO sale_items (sale_id, product_id, quantity, unit_price) VALUES (@saleId, @productId, 2, 8.50);
  SELECT @productId = product_id FROM products WHERE code = N'P-007';
  INSERT INTO sale_items (sale_id, product_id, quantity, unit_price) VALUES (@saleId, @productId, 1, 2.50);

  INSERT INTO sales (sale_number, payment_method, total, sold_at) VALUES (N'#1045', N'Plin', 51.90, DATEADD(MINUTE, -127, SYSUTCDATETIME()));
  SET @saleId = SCOPE_IDENTITY();
  SELECT @productId = product_id FROM products WHERE code = N'P-004';
  INSERT INTO sale_items (sale_id, product_id, quantity, unit_price) VALUES (@saleId, @productId, 3, 4.20);
  SELECT @productId = product_id FROM products WHERE code = N'P-005';
  INSERT INTO sale_items (sale_id, product_id, quantity, unit_price) VALUES (@saleId, @productId, 1, 22.90);
  SELECT @productId = product_id FROM products WHERE code = N'P-008';
  INSERT INTO sale_items (sale_id, product_id, quantity, unit_price) VALUES (@saleId, @productId, 1, 2.00);

  INSERT INTO sales (sale_number, payment_method, total, sold_at) VALUES (N'#1044', N'Efectivo', 7.50, DATEADD(MINUTE, -153, SYSUTCDATETIME()));
  SET @saleId = SCOPE_IDENTITY();
  SELECT @productId = product_id FROM products WHERE code = N'P-007';
  INSERT INTO sale_items (sale_id, product_id, quantity, unit_price) VALUES (@saleId, @productId, 3, 2.50);

  INSERT INTO sales (sale_number, payment_method, total, sold_at) VALUES (N'#1043', N'Tarjeta', 83.20, DATEADD(MINUTE, -176, SYSUTCDATETIME()));
  SET @saleId = SCOPE_IDENTITY();
  SELECT @productId = product_id FROM products WHERE code = N'P-010';
  INSERT INTO sale_items (sale_id, product_id, quantity, unit_price) VALUES (@saleId, @productId, 2, 11.90);
  SELECT @productId = product_id FROM products WHERE code = N'P-011';
  INSERT INTO sale_items (sale_id, product_id, quantity, unit_price) VALUES (@saleId, @productId, 2, 6.30);
  SELECT @productId = product_id FROM products WHERE code = N'P-012';
  INSERT INTO sale_items (sale_id, product_id, quantity, unit_price) VALUES (@saleId, @productId, 3, 9.90);
END;
