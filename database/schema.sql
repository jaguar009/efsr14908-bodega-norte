/* Modelo SQL Server de referencia para Bodega Norte */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE categories (
  category_id INT IDENTITY(1,1) PRIMARY KEY,
  name NVARCHAR(80) NOT NULL UNIQUE,
  is_active BIT NOT NULL DEFAULT 1
);

CREATE TABLE suppliers (
  supplier_id INT IDENTITY(1,1) PRIMARY KEY,
  name NVARCHAR(120) NOT NULL,
  contact_name NVARCHAR(120) NULL,
  phone NVARCHAR(30) NULL,
  is_active BIT NOT NULL DEFAULT 1
);

CREATE TABLE products (
  product_id INT IDENTITY(1,1) PRIMARY KEY,
  code NVARCHAR(30) NOT NULL UNIQUE,
  name NVARCHAR(160) NOT NULL,
  category_id INT NOT NULL,
  supplier_id INT NULL,
  unit NVARCHAR(30) NOT NULL,
  stock DECIMAL(12,2) NOT NULL DEFAULT 0 CHECK (stock >= 0),
  min_stock DECIMAL(12,2) NOT NULL DEFAULT 0 CHECK (min_stock >= 0),
  cost DECIMAL(12,2) NOT NULL CHECK (cost >= 0),
  sale_price DECIMAL(12,2) NOT NULL CHECK (sale_price > 0),
  is_active BIT NOT NULL DEFAULT 1,
  updated_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_products_categories FOREIGN KEY (category_id) REFERENCES categories(category_id),
  CONSTRAINT FK_products_suppliers FOREIGN KEY (supplier_id) REFERENCES suppliers(supplier_id)
);

CREATE TABLE sales (
  sale_id INT IDENTITY(1,1) PRIMARY KEY,
  sale_number NVARCHAR(30) NOT NULL UNIQUE,
  payment_method NVARCHAR(30) NOT NULL CHECK (payment_method IN ('Efectivo', 'Yape', 'Plin', 'Tarjeta')),
  total DECIMAL(12,2) NOT NULL CHECK (total >= 0),
  sold_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE sale_items (
  sale_item_id INT IDENTITY(1,1) PRIMARY KEY,
  sale_id INT NOT NULL,
  product_id INT NOT NULL,
  quantity DECIMAL(12,2) NOT NULL CHECK (quantity > 0),
  unit_price DECIMAL(12,2) NOT NULL CHECK (unit_price > 0),
  line_total AS (quantity * unit_price) PERSISTED,
  CONSTRAINT FK_sale_items_sales FOREIGN KEY (sale_id) REFERENCES sales(sale_id),
  CONSTRAINT FK_sale_items_products FOREIGN KEY (product_id) REFERENCES products(product_id)
);

CREATE INDEX IX_products_low_stock ON products(stock, min_stock) WHERE is_active = 1;
CREATE INDEX IX_sales_sold_at ON sales(sold_at);
