-- Datos de demostración ficticios. Seguro de volver a ejecutar.

INSERT INTO bodega_norte.categories (name) VALUES
  ('Lácteos'), ('Panadería'), ('Abarrotes'), ('Bebidas'),
  ('Snacks'), ('Enlatados'), ('Limpieza')
ON CONFLICT (name) DO NOTHING;

INSERT INTO bodega_norte.suppliers (name, contact_name, phone) VALUES
  ('Distribuidora Central', 'María Paredes', '987 654 321'),
  ('Alimentos del Valle', 'Luis Mendoza', '986 212 480'),
  ('Comercial San Luis', 'Rosa Salazar', '985 730 118'),
  ('Bebidas Andinas', 'Carlos Ruiz', '982 440 607'),
  ('Granja Santa Rosa', 'Ana Torres', '981 330 929')
ON CONFLICT (name) DO NOTHING;

INSERT INTO bodega_norte.products
  (code, name, category_id, supplier_id, unit, stock, min_stock, cost, sale_price)
SELECT v.code, v.name, c.category_id, s.supplier_id, v.unit, v.stock, v.min_stock, v.cost, v.sale_price
FROM (VALUES
  ('P-001', 'Leche Gloria 1L', 'Lácteos', 'Distribuidora Central', 'unidad', 3.00, 10.00, 3.25, 4.50),
  ('P-002', 'Pan de molde Bimbo', 'Panadería', 'Alimentos del Valle', 'unidad', 5.00, 10.00, 6.20, 8.50),
  ('P-003', 'Aceite Primor 1L', 'Abarrotes', 'Distribuidora Central', 'unidad', 4.00, 8.00, 7.80, 10.50),
  ('P-004', 'Azúcar Rubia 1kg', 'Abarrotes', 'Comercial San Luis', 'unidad', 6.00, 10.00, 3.10, 4.20),
  ('P-005', 'Arroz Costeño 5kg', 'Abarrotes', 'Comercial San Luis', 'unidad', 2.00, 8.00, 17.50, 22.90),
  ('P-006', 'Gaseosa Inca Kola 1.5L', 'Bebidas', 'Bebidas Andinas', 'unidad', 24.00, 12.00, 5.80, 8.50),
  ('P-007', 'Agua San Luis 625ml', 'Bebidas', 'Bebidas Andinas', 'unidad', 32.00, 15.00, 1.50, 2.50),
  ('P-008', 'Galletas Casino Chocolate', 'Snacks', 'Alimentos del Valle', 'paquete', 18.00, 10.00, 1.20, 2.00),
  ('P-009', 'Atún Florida 170g', 'Enlatados', 'Distribuidora Central', 'lata', 11.00, 6.00, 4.20, 6.20),
  ('P-010', 'Huevos Pardos x 15', 'Lácteos', 'Granja Santa Rosa', 'bandeja', 9.00, 6.00, 8.80, 11.90),
  ('P-011', 'Detergente Bolívar 500g', 'Limpieza', 'Comercial San Luis', 'unidad', 14.00, 8.00, 4.40, 6.30),
  ('P-012', 'Papel higiénico Elite x 4', 'Limpieza', 'Alimentos del Valle', 'paquete', 7.00, 6.00, 7.10, 9.90)
) AS v(code, name, category, supplier, unit, stock, min_stock, cost, sale_price)
INNER JOIN bodega_norte.categories c ON c.name = v.category
INNER JOIN bodega_norte.suppliers s ON s.name = v.supplier
ON CONFLICT (code) DO NOTHING;

INSERT INTO bodega_norte.sales (sale_number, payment_method, total, sold_at) VALUES
  ('1048', 'Yape', 32.00, now() - INTERVAL '30 minutes'),
  ('1047', 'Efectivo', 46.10, now() - INTERVAL '58 minutes'),
  ('1046', 'Efectivo', 19.50, now() - INTERVAL '102 minutes'),
  ('1045', 'Plin', 37.50, now() - INTERVAL '127 minutes'),
  ('1044', 'Efectivo', 7.50, now() - INTERVAL '153 minutes'),
  ('1043', 'Tarjeta', 66.10, now() - INTERVAL '176 minutes')
ON CONFLICT (sale_number) DO NOTHING;

INSERT INTO bodega_norte.sale_items (sale_id, product_id, quantity, unit_price)
SELECT s.sale_id, p.product_id, v.quantity, v.unit_price
FROM (VALUES
  ('1048', 'P-001', 2.00, 4.50), ('1048', 'P-006', 2.00, 8.50), ('1048', 'P-008', 3.00, 2.00),
  ('1047', 'P-002', 2.00, 8.50), ('1047', 'P-003', 1.00, 10.50), ('1047', 'P-009', 3.00, 6.20),
  ('1046', 'P-006', 2.00, 8.50), ('1046', 'P-007', 1.00, 2.50),
  ('1045', 'P-004', 3.00, 4.20), ('1045', 'P-005', 1.00, 22.90), ('1045', 'P-008', 1.00, 2.00),
  ('1044', 'P-007', 3.00, 2.50),
  ('1043', 'P-010', 2.00, 11.90), ('1043', 'P-011', 2.00, 6.30), ('1043', 'P-012', 3.00, 9.90)
) AS v(sale_number, code, quantity, unit_price)
INNER JOIN bodega_norte.sales s ON s.sale_number = v.sale_number
INNER JOIN bodega_norte.products p ON p.code = v.code
WHERE NOT EXISTS (
  SELECT 1 FROM bodega_norte.sale_items si
  WHERE si.sale_id = s.sale_id AND si.product_id = p.product_id
);
