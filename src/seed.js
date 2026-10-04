export const initialProducts = [
  { id: 'P-001', name: 'Leche Gloria 1L', category: 'Lácteos', stock: 3, minStock: 10, cost: 3.25, price: 4.5, supplier: 'Distribuidora Central', unit: 'unidad', updated: 'Hoy, 08:42' },
  { id: 'P-002', name: 'Pan de molde Bimbo', category: 'Panadería', stock: 5, minStock: 10, cost: 6.2, price: 8.5, supplier: 'Alimentos del Valle', unit: 'unidad', updated: 'Ayer, 17:30' },
  { id: 'P-003', name: 'Aceite Primor 1L', category: 'Abarrotes', stock: 4, minStock: 8, cost: 7.8, price: 10.5, supplier: 'Distribuidora Central', unit: 'unidad', updated: 'Hoy, 09:10' },
  { id: 'P-004', name: 'Azúcar Rubia 1kg', category: 'Abarrotes', stock: 6, minStock: 10, cost: 3.1, price: 4.2, supplier: 'Comercial San Luis', unit: 'unidad', updated: 'Ayer, 16:05' },
  { id: 'P-005', name: 'Arroz Costeño 5kg', category: 'Abarrotes', stock: 2, minStock: 8, cost: 17.5, price: 22.9, supplier: 'Comercial San Luis', unit: 'unidad', updated: 'Hoy, 07:56' },
  { id: 'P-006', name: 'Gaseosa Inca Kola 1.5L', category: 'Bebidas', stock: 24, minStock: 12, cost: 5.8, price: 8.5, supplier: 'Bebidas Andinas', unit: 'unidad', updated: 'Hoy, 10:18' },
  { id: 'P-007', name: 'Agua San Luis 625ml', category: 'Bebidas', stock: 32, minStock: 15, cost: 1.5, price: 2.5, supplier: 'Bebidas Andinas', unit: 'unidad', updated: 'Hoy, 10:04' },
  { id: 'P-008', name: 'Galletas Casino Chocolate', category: 'Snacks', stock: 18, minStock: 10, cost: 1.2, price: 2.0, supplier: 'Alimentos del Valle', unit: 'paquete', updated: 'Ayer, 18:10' },
  { id: 'P-009', name: 'Atún Florida 170g', category: 'Enlatados', stock: 11, minStock: 6, cost: 4.2, price: 6.2, supplier: 'Distribuidora Central', unit: 'lata', updated: 'Ayer, 14:12' },
  { id: 'P-010', name: 'Huevos Pardos x 15', category: 'Lácteos', stock: 9, minStock: 6, cost: 8.8, price: 11.9, supplier: 'Granja Santa Rosa', unit: 'bandeja', updated: 'Hoy, 08:10' },
  { id: 'P-011', name: 'Detergente Bolívar 500g', category: 'Limpieza', stock: 14, minStock: 8, cost: 4.4, price: 6.3, supplier: 'Comercial San Luis', unit: 'unidad', updated: 'Ayer, 12:48' },
  { id: 'P-012', name: 'Papel higiénico Elite x 4', category: 'Limpieza', stock: 7, minStock: 6, cost: 7.1, price: 9.9, supplier: 'Alimentos del Valle', unit: 'paquete', updated: 'Hoy, 09:22' },
];

export const initialSales = [
  { id: '#1048', time: '10:24', customer: 'Venta mostrador', items: 7, total: 32.0, method: 'Yape' },
  { id: '#1047', time: '09:56', customer: 'Venta mostrador', items: 6, total: 46.1, method: 'Efectivo' },
  { id: '#1046', time: '09:12', customer: 'Venta mostrador', items: 3, total: 19.5, method: 'Efectivo' },
  { id: '#1045', time: '08:47', customer: 'Venta mostrador', items: 5, total: 37.5, method: 'Plin' },
  { id: '#1044', time: '08:21', customer: 'Venta mostrador', items: 1, total: 7.5, method: 'Efectivo' },
  { id: '#1043', time: '07:58', customer: 'Venta mostrador', items: 7, total: 66.1, method: 'Tarjeta' },
];

export const suppliers = [
  { name: 'Distribuidora Central', contact: 'María Paredes', phone: '987 654 321', products: 4, status: 'Activo' },
  { name: 'Alimentos del Valle', contact: 'Luis Mendoza', phone: '986 212 480', products: 3, status: 'Activo' },
  { name: 'Comercial San Luis', contact: 'Rosa Salazar', phone: '985 730 118', products: 3, status: 'Activo' },
  { name: 'Bebidas Andinas', contact: 'Carlos Ruiz', phone: '982 440 607', products: 2, status: 'Activo' },
  { name: 'Granja Santa Rosa', contact: 'Ana Torres', phone: '981 330 929', products: 1, status: 'Activo' },
];
