from pathlib import Path
from docx import Document
from docx.shared import Cm, Pt, Inches, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_BREAK
from docx.enum.section import WD_SECTION
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_CELL_VERTICAL_ALIGNMENT
from docx.oxml import OxmlElement
from docx.oxml.ns import qn


ROOT = Path(__file__).resolve().parent
OUT = ROOT / "docs" / "word"
OUT.mkdir(parents=True, exist_ok=True)

COURSE = "EFSR14908 – Experiencia Formativa en Situación Real de Trabajo"
PROJECT = "Sistema de inventario y ventas para la bodega Bodega Norte"
TEACHER = "Judith Jiménez Monago"
PERIOD = "Periodo académico 2026"
BLUE = "10294B"
BLUE2 = "1F4E79"
LIGHT = "EAF0F6"
GRAY = "D9E1EA"
TEXT = "222222"


def set_cell_shading(cell, fill):
    tc_pr = cell._tc.get_or_add_tcPr()
    shd = tc_pr.find(qn("w:shd"))
    if shd is None:
        shd = OxmlElement("w:shd")
        tc_pr.append(shd)
    shd.set(qn("w:fill"), fill)


def set_cell_borders(cell, color="D9D9D9", size="4"):
    tc = cell._tc
    tc_pr = tc.get_or_add_tcPr()
    borders = tc_pr.first_child_found_in("w:tcBorders")
    if borders is None:
        borders = OxmlElement("w:tcBorders")
        tc_pr.append(borders)
    for edge in ("top", "left", "bottom", "right", "insideH", "insideV"):
        tag = "w:" + edge
        element = borders.find(qn(tag))
        if element is None:
            element = OxmlElement(tag)
            borders.append(element)
        element.set(qn("w:val"), "single")
        element.set(qn("w:sz"), size)
        element.set(qn("w:space"), "0")
        element.set(qn("w:color"), color)


def set_cell_margins(cell, top=80, start=100, bottom=80, end=100):
    tc = cell._tc
    tc_pr = tc.get_or_add_tcPr()
    tc_mar = tc_pr.first_child_found_in("w:tcMar")
    if tc_mar is None:
        tc_mar = OxmlElement("w:tcMar")
        tc_pr.append(tc_mar)
    for m, v in (("top", top), ("start", start), ("bottom", bottom), ("end", end)):
        node = tc_mar.find(qn(f"w:{m}"))
        if node is None:
            node = OxmlElement(f"w:{m}")
            tc_mar.append(node)
        node.set(qn("w:w"), str(v))
        node.set(qn("w:type"), "dxa")


def set_repeat_table_header(row):
    tr_pr = row._tr.get_or_add_trPr()
    tbl_header = OxmlElement("w:tblHeader")
    tbl_header.set(qn("w:val"), "true")
    tr_pr.append(tbl_header)


def add_page_field(paragraph):
    run = paragraph.add_run()
    fld_char1 = OxmlElement("w:fldChar")
    fld_char1.set(qn("w:fldCharType"), "begin")
    instr_text = OxmlElement("w:instrText")
    instr_text.set(qn("xml:space"), "preserve")
    instr_text.text = " PAGE "
    fld_char2 = OxmlElement("w:fldChar")
    fld_char2.set(qn("w:fldCharType"), "end")
    run._r.append(fld_char1)
    run._r.append(instr_text)
    run._r.append(fld_char2)


def style_document(doc):
    sec = doc.sections[0]
    sec.page_width = Cm(21)
    sec.page_height = Cm(29.7)
    sec.top_margin = Cm(3)
    sec.bottom_margin = Cm(3)
    sec.left_margin = Cm(2.5)
    sec.right_margin = Cm(2.5)
    styles = doc.styles
    normal = styles["Normal"]
    normal.font.name = "Arial"
    normal._element.rPr.rFonts.set(qn("w:eastAsia"), "Arial")
    normal.font.size = Pt(11)
    normal.font.color.rgb = RGBColor.from_string(TEXT)
    normal.paragraph_format.space_after = Pt(6)
    normal.paragraph_format.line_spacing = 1.0
    for name, size, color in (("Title", 20, BLUE), ("Heading 1", 16, BLUE), ("Heading 2", 13, BLUE2), ("Heading 3", 11, BLUE2)):
        st = styles[name]
        st.font.name = "Arial"
        st._element.rPr.rFonts.set(qn("w:eastAsia"), "Arial")
        st.font.size = Pt(size)
        st.font.bold = True
        st.font.color.rgb = RGBColor.from_string(color)
        st.paragraph_format.space_before = Pt(10)
        st.paragraph_format.space_after = Pt(5)
    for section in doc.sections:
        header = section.header.paragraphs[0]
        header.text = "BODEGA NORTE  |  EFSR14908"
        header.alignment = WD_ALIGN_PARAGRAPH.RIGHT
        header.runs[0].font.name = "Arial"
        header.runs[0].font.size = Pt(8)
        header.runs[0].font.color.rgb = RGBColor.from_string("6B7280")
        footer = section.footer.paragraphs[0]
        footer.alignment = WD_ALIGN_PARAGRAPH.CENTER
        run = footer.add_run("EFSR14908  ·  Documento editable  ·  Página ")
        run.font.name = "Arial"
        run.font.size = Pt(8)
        run.font.color.rgb = RGBColor.from_string("6B7280")
        add_page_field(footer)


def title(doc, text, subtitle=None):
    p = doc.add_paragraph(style="Title")
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.add_run(text)
    if subtitle:
        p2 = doc.add_paragraph()
        p2.alignment = WD_ALIGN_PARAGRAPH.CENTER
        r = p2.add_run(subtitle)
        r.font.name = "Arial"
        r.font.size = Pt(12)
        r.font.color.rgb = RGBColor.from_string(BLUE2)


def cover(doc, doc_title, deliverable=None):
    doc.add_paragraph("", style="Normal")
    doc.add_paragraph("", style="Normal")
    title(doc, doc_title, PROJECT)
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run("DOCUMENTO ACADÉMICO EDITABLE")
    r.bold = True
    r.font.name = "Arial"
    r.font.size = Pt(11)
    r.font.color.rgb = RGBColor.from_string(BLUE)
    doc.add_paragraph("")
    info = [
        ("Curso", COURSE),
        ("Docente", TEACHER),
        ("Periodo", PERIOD),
        ("Entregable", deliverable or "Documento de soporte"),
        ("Estudiante / integrantes", "[Completar]"),
        ("Coordinador del proyecto", "[Completar]"),
        ("Institución / bodega", "Bodega Norte – [completar ubicación]"),
        ("Fecha de entrega", "[Completar]"),
    ]
    t = doc.add_table(rows=0, cols=2)
    t.alignment = WD_TABLE_ALIGNMENT.CENTER
    t.autofit = True
    for i, (k, v) in enumerate(info):
        cells = t.add_row().cells
        cells[0].text = k
        cells[1].text = v
        set_cell_shading(cells[0], LIGHT)
        for c in cells:
            set_cell_borders(c)
            set_cell_margins(c)
            c.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER
            for par in c.paragraphs:
                for run in par.runs:
                    run.font.name = "Arial"
                    run.font.size = Pt(10)
        for run in cells[0].paragraphs[0].runs:
            run.bold = True
            run.font.color.rgb = RGBColor.from_string(BLUE)
    doc.add_paragraph("")
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run("Elaborado a partir de la estructura y criterios indicados en los documentos de referencia del curso.")
    r.italic = True
    r.font.size = Pt(9)
    r.font.color.rgb = RGBColor.from_string("6B7280")
    doc.add_page_break()


def h(doc, text, level=1):
    doc.add_heading(text, level=level)


def para(doc, text, bold_prefix=None):
    p = doc.add_paragraph()
    if bold_prefix and text.startswith(bold_prefix):
        r = p.add_run(bold_prefix)
        r.bold = True
        p.add_run(text[len(bold_prefix):])
    else:
        p.add_run(text)
    return p


def bullets(doc, items, level=0):
    for item in items:
        p = doc.add_paragraph(style="List Bullet" if level == 0 else "List Bullet 2")
        p.add_run(item)


def numbered(doc, items):
    for item in items:
        p = doc.add_paragraph(style="List Number")
        p.add_run(item)


def table(doc, headers, rows, widths=None):
    t = doc.add_table(rows=1, cols=len(headers))
    t.alignment = WD_TABLE_ALIGNMENT.CENTER
    t.style = "Table Grid"
    hdr = t.rows[0]
    set_repeat_table_header(hdr)
    for i, value in enumerate(headers):
        cell = hdr.cells[i]
        cell.text = str(value)
        set_cell_shading(cell, BLUE)
        set_cell_borders(cell, BLUE)
        set_cell_margins(cell)
        for p in cell.paragraphs:
            for r in p.runs:
                r.font.name = "Arial"
                r.font.size = Pt(9)
                r.font.bold = True
                r.font.color.rgb = RGBColor(255, 255, 255)
    for ridx, row in enumerate(rows):
        cells = t.add_row().cells
        for i, value in enumerate(row):
            cells[i].text = str(value)
            if ridx % 2 == 0:
                set_cell_shading(cells[i], "F7F9FB")
            set_cell_borders(cells[i])
            set_cell_margins(cells[i])
            cells[i].vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER
            for p in cells[i].paragraphs:
                for r in p.runs:
                    r.font.name = "Arial"
                    r.font.size = Pt(9)
                    r.font.color.rgb = RGBColor.from_string(TEXT)
        if widths:
            for i, width in enumerate(widths):
                cells[i].width = Cm(width)
    doc.add_paragraph("")
    return t


def image(doc, path, caption=None, width=6.5):
    pth = ROOT / path
    if not pth.exists():
        para(doc, f"Evidencia pendiente de insertar: {path}")
        return
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.add_run().add_picture(str(pth), width=Inches(width))
    if caption:
        cp = doc.add_paragraph()
        cp.alignment = WD_ALIGN_PARAGRAPH.CENTER
        r = cp.add_run(caption)
        r.italic = True
        r.font.size = Pt(9)
        r.font.color.rgb = RGBColor.from_string("6B7280")


def save(doc, filename):
    style_document(doc)
    doc.save(OUT / filename)


def doc_01():
    d = Document(); cover(d, "Informe de proyecto – Entregable 1", "Entregable 1: capítulos I y II / diagnóstico y propuesta inicial")
    h(d, "1. Resumen ejecutivo")
    para(d, "Bodega Norte es una propuesta de sistema web para registrar productos, controlar existencias y gestionar ventas en una bodega minorista. El proyecto responde a la necesidad de contar con información confiable y oportuna para reducir errores de registro, quiebres de stock y tiempos de atención.")
    para(d, "La primera entrega define el problema, el contexto, los objetivos, la justificación, los beneficiarios, el alcance y la viabilidad. El producto se plantea como un MVP demostrable, escalable hacia SQL Server, integración con Watson Assistant y despliegue institucional.")
    h(d, "2. Introducción")
    para(d, "La gestión manual o dispersa de inventario dificulta saber cuánto producto queda, qué artículos requieren reposición y cuáles son las ventas del periodo. En una bodega, estas decisiones deben tomarse con rapidez y con información simple de interpretar.")
    para(d, "El proyecto se desarrollará aplicando principios ágiles y entregas incrementales. El alcance inicial comprende catálogo de productos, entradas y salidas, venta rápida, alertas de stock bajo, reportes y una interfaz conversacional preparada para Watson Assistant.")
    h(d, "3. Diagnóstico del entorno – SEPTE")
    table(d, ["Variable", "Situación observada", "Implicancia para el proyecto"], [
        ("Social", "La atención es directa y los clientes esperan rapidez.", "La venta debe registrarse en pocos pasos y mostrar un resumen claro."),
        ("Económica", "La pérdida por vencimiento, merma o falta de reposición afecta el margen.", "Se requieren alertas, control de cantidades y reportes de ventas."),
        ("Tecnológica", "La operación puede apoyarse en una aplicación web de bajo costo.", "Se propone una interfaz responsiva y una base preparada para SQL Server."),
        ("Ambiental", "El control de vencimientos y mermas reduce desperdicios.", "El registro de productos y movimientos debe permitir decisiones de reposición."),
        ("Político-legal", "La información de ventas debe manejarse con responsabilidad y trazabilidad.", "Se deben separar roles, proteger credenciales y documentar futuras integraciones."),
    ], [2.5, 6, 7])
    h(d, "4. Planteamiento del problema")
    para(d, "Problema central: Bodega Norte no dispone de un mecanismo único y oportuno para controlar inventario y registrar ventas, lo que genera información desactualizada, errores operativos y dificultad para decidir qué reponer.")
    h(d, "5. Objetivos")
    h(d, "5.1 Objetivo general", 2)
    para(d, "Diseñar e implementar un sistema de inventario y ventas para Bodega Norte que permita registrar productos y operaciones, controlar existencias y obtener información útil para la toma de decisiones.")
    h(d, "5.2 Objetivos específicos SMART", 2)
    numbered(d, [
        "Implementar durante el ciclo del proyecto un catálogo con al menos 10 productos y campos de stock actual, stock mínimo, precio y categoría.",
        "Registrar y validar ventas con actualización inmediata de existencias y un comprobante interno de la operación.",
        "Entregar reportes operativos de ventas, productos con stock bajo y valorización referencial del inventario.",
        "Documentar instalación, uso, arquitectura, pruebas e integración conversacional para permitir continuidad del proyecto.",
    ])
    h(d, "6. Justificación y beneficiarios")
    para(d, "La solución centraliza la información y convierte operaciones cotidianas en datos consultables. La propuesta es pertinente para el curso porque integra análisis, diseño, programación, pruebas, documentación y sustentación.")
    table(d, ["Beneficiario", "Beneficio esperado"], [
        ("Propietario o administrador", "Visibilidad de ventas, stock bajo y productos valorizados."),
        ("Personal de atención", "Registro de venta guiado y reducción de cálculos manuales."),
        ("Clientes", "Menor tiempo de espera y mayor disponibilidad de productos."),
        ("Equipo del curso", "Aplicación de Scrum, diseño, desarrollo, pruebas y documentación."),
    ])
    h(d, "7. Modelo Canvas de la propuesta")
    table(d, ["Bloque", "Definición para Bodega Norte"], [
        ("Problema", "Control manual, errores de stock y falta de reportes."),
        ("Propuesta de valor", "Registrar ventas e inventario en una interfaz rápida, clara y escalable."),
        ("Segmento", "Bodegas minoristas y pequeños comercios."),
        ("Canales", "Aplicación web local o institucional; futura publicación interna."),
        ("Relación", "Uso autoservicio, alertas y asistencia conversacional."),
        ("Ingresos / ahorro", "Ahorro de tiempo, reducción de mermas y mejor reposición."),
        ("Recursos clave", "Aplicación, base de datos, documentación y datos de productos."),
        ("Actividades clave", "Registrar productos, ventas, movimientos, reportes y pruebas."),
        ("Aliados", "Docente, equipo de estudiantes, proveedor de productos y responsable de la bodega."),
    ])
    h(d, "8. Alcance y delimitación")
    h(d, "8.1 Incluye", 2)
    bullets(d, ["Dashboard con indicadores de ventas, stock bajo y actividad reciente.", "Catálogo de productos con alta, búsqueda, filtro y stock mínimo.", "Venta rápida con selección de productos, cantidades, cálculo de total y método de pago.", "Actualización del stock y registro histórico de ventas.", "Reportes operativos exportables y documentos de instalación, usuario, técnico y pruebas."])
    h(d, "8.2 No incluye en el MVP", 2)
    bullets(d, ["Facturación electrónica, conexión bancaria real o integración con SUNAT.", "Control multi-sucursal, compras con proveedores y contabilidad.", "Autenticación institucional conectada a un servidor productivo.", "Integración productiva con Watson Assistant: se entrega el contrato y mapa de intents para una fase posterior."])
    h(d, "9. Riesgos, interesados y viabilidad")
    table(d, ["Riesgo", "Probabilidad / impacto", "Respuesta"], [
        ("Datos de productos incompletos", "Media / Media", "Usar plantilla de carga y validaciones de campos obligatorios."),
        ("Cambios en requisitos", "Media / Alta", "Priorizar historias y registrar acuerdos en cada revisión."),
        ("Pérdida de información local", "Baja / Alta", "Exportar respaldos CSV y preparar persistencia SQL Server."),
        ("Credenciales de integración expuestas", "Baja / Alta", "Usar variables de entorno y no versionar secretos."),
        ("Tiempo limitado para completar todo", "Media / Alta", "Entregar por hitos: inicial, 50% y final."),
    ])
    table(d, ["Interesado", "Interés / responsabilidad"], [
        ("Docente", "Validar el avance y evaluar entregables y sustentación."),
        ("Equipo de estudiantes", "Analizar, construir, probar y documentar la solución."),
        ("Responsable de Bodega Norte", "Validar flujo de venta, catálogo y reportes."),
        ("Usuario de caja", "Operar el registro de productos y ventas."),
    ])
    para(d, "Viabilidad: la propuesta es técnica y económicamente viable como MVP, porque utiliza una interfaz web y puede operar con datos de demostración. Para una puesta en producción se deberá incorporar backend, base de datos SQL Server, autenticación, copias de seguridad y pruebas de aceptación con usuarios.")
    h(d, "10. Productos y entregables previstos")
    table(d, ["Producto", "Evidencia / formato"], [
        ("Aplicación web", "Código fuente editable y demo funcional."),
        ("Base de datos", "database/schema.sql y modelo de datos."),
        ("Documentación académica", "Informes Word de entregable 1, avance 50% y entrega final."),
        ("Documentación operativa", "Manual de usuario y guía de instalación en Word."),
        ("Documentación técnica", "Manual técnico, matriz de pruebas, cronograma, guion e integración Watson en Word."),
    ])
    h(d, "11. Aprobación de la entrega")
    table(d, ["Rol", "Nombre / firma", "Fecha"], [("Estudiante / equipo", "[Completar]", "[Completar]"), ("Docente", TEACHER, "[Completar]"), ("Responsable de la organización", "[Completar]", "[Completar]")])
    save(d, "01_Entregable_1_Informe_Inicial.docx")


def doc_02():
    d = Document(); cover(d, "Informe de avance al 50% – Entregable 2", "Entregable 2: diseño, construcción inicial, pruebas y defensa grupal")
    h(d, "1. Estado del proyecto")
    para(d, "Al corte del 50%, el proyecto cuenta con una interfaz funcional para dashboard, ventas, inventario, productos, proveedores, reportes y configuración. Se han priorizado los flujos que demuestran valor: revisar stock, registrar una venta y consultar alertas.")
    table(d, ["Área", "Estado al 50%", "Evidencia"], [
        ("Interfaz", "Completada para desktop y adaptada a móvil.", "Capturas visuales y navegación en la aplicación."),
        ("Ventas", "Flujo de venta validado con actualización de stock.", "Caso IT-02 y mensaje de venta registrada."),
        ("Inventario", "Listado, búsqueda y filtro por stock bajo.", "Tabla de inventario y filtro Bajo."),
        ("Reportes", "Resumen de ventas y productos con bajo stock.", "Módulo Reportes."),
        ("Persistencia", "LocalStorage en el demo; esquema SQL Server preparado.", "database/schema.sql y manual técnico."),
        ("Asistente", "Contrato de Watson Assistant documentado; demo offline disponible.", "Documento de integración y manual técnico."),
    ])
    h(d, "2. Historias de usuario priorizadas")
    table(d, ["ID", "Historia", "Criterio de aceptación", "Estado"], [
        ("US-01", "Como administrador, quiero ver indicadores del día.", "Se muestran ventas, ingresos, stock bajo y productos.", "Listo"),
        ("US-02", "Como encargado, quiero registrar productos.", "Nombre, categoría, precio, stock y mínimo son editables.", "Listo"),
        ("US-03", "Como cajero, quiero buscar productos y vender.", "El total se calcula y el stock disminuye al confirmar.", "Listo"),
        ("US-04", "Como administrador, quiero conocer el stock bajo.", "El filtro Bajo muestra productos por debajo del mínimo.", "Listo"),
        ("US-05", "Como responsable, quiero exportar un reporte.", "Se genera un archivo CSV con ventas y productos.", "Listo"),
        ("US-06", "Como usuario, quiero consultar al asistente.", "La intención stock_bajo devuelve una respuesta útil.", "Contrato listo"),
    ])
    h(d, "3. Diseño funcional y técnico")
    para(d, "La solución se organiza en una capa de presentación React, un estado local para el demo y una capa de datos preparada para migrar a SQL Server. La navegación principal se mantiene visible en escritorio y se convierte en un menú en pantallas pequeñas.")
    table(d, ["Módulo", "Responsabilidad", "Regla clave"], [
        ("Dashboard", "Indicadores y actividad reciente.", "Los valores se calculan a partir de ventas y productos."),
        ("Ventas", "Carrito y confirmación de operación.", "No se permite superar el stock disponible."),
        ("Inventario", "Consulta y filtro de existencias.", "Stock bajo cuando actual < mínimo."),
        ("Productos", "Alta y edición del catálogo.", "Precio, stock y mínimo deben ser válidos."),
        ("Reportes", "Indicadores y exportación.", "CSV editable para análisis posterior."),
    ])
    h(d, "4. Metodología y planificación")
    para(d, "Se utiliza Scrum con ciclos cortos, revisión del avance y priorización por valor. El entregable 1 concentra el problema y la propuesta; el entregable 2 demuestra el diseño y la construcción al 50%; la entrega final integra producto, componentes, recursos, pruebas y sustentación.")
    table(d, ["Rol", "Responsabilidad"], [
        ("Product Owner", "Priorizar historias y validar el valor para la bodega."),
        ("Scrum Master", "Facilitar acuerdos, seguimiento y eliminación de impedimentos."),
        ("Equipo de desarrollo", "Diseño, código, pruebas, documentación e integración."),
        ("Usuario validador", "Revisar flujo de caja, inventario y reportes."),
    ])
    h(d, "5. Plan de pruebas al 50%")
    table(d, ["Caso", "Resultado esperado", "Resultado actual"], [
        ("IT-01", "Filtrar productos con stock bajo.", "OK: se visualizan cinco productos de demostración."),
        ("IT-02", "Registrar venta y actualizar stock.", "OK: venta registrada y stock disminuido."),
        ("IT-03", "Validar producto con campos incompletos.", "OK: botón Guardar permanece deshabilitado."),
        ("IT-04", "Abrir menú en móvil.", "OK: navegación accesible mediante menú."),
        ("IT-05", "Consultar asistente por stock bajo.", "OK: devuelve lista resumida de productos."),
    ])
    h(d, "6. Plan de integración continua")
    numbered(d, [
        "Instalar dependencias con pnpm install.",
        "Ejecutar la construcción de producción con pnpm run build.",
        "Ejecutar pruebas de integración Java cuando se modifique el flujo de venta o el contrato de datos.",
        "Revisar que los documentos, diagramas y evidencias estén actualizados antes de cada entrega.",
        "No incorporar secretos: mantener credenciales en variables de entorno y usar .env.example como referencia.",
    ])
    h(d, "7. Riesgos y acciones pendientes")
    bullets(d, ["Conectar el MVP a un backend y SQL Server para operación multiusuario.", "Completar pruebas de aceptación con datos reales autorizados por la organización.", "Validar el mapa de intents y entidades con un asistente Watson configurado.", "Completar nombres, firmas, ubicación, integrantes y fecha en la portada."])
    h(d, "8. Evidencias visuales")
    image(d, "docs/evidencias/EVID_visual_escritorio.png", "Evidencia E-01: interfaz responsive en escritorio", 6.5)
    image(d, "docs/evidencias/EVID_visual_movil.png", "Evidencia E-02: interfaz responsive en móvil", 3.2)
    save(d, "02_Entregable_2_Avance_50.docx")


def doc_03():
    d = Document(); cover(d, "Informe final de proyecto – Entregable 3", "Entrega final: producto, componentes, recursos, pruebas y defensa")
    h(d, "1. Resumen")
    para(d, "El proyecto Bodega Norte entrega un sistema de inventario y ventas orientado a una bodega minorista. El producto permite visualizar indicadores, gestionar productos, revisar existencias, registrar ventas, identificar stock bajo y consultar reportes. La solución se presenta como MVP funcional y documentado, con una ruta de evolución hacia SQL Server, autenticación institucional y Watson Assistant.")
    h(d, "2. Introducción")
    para(d, "La solución fue construida para responder a un problema operativo concreto: la dificultad de mantener información actualizada sobre productos, existencias y ventas. El desarrollo se organizó por entregables y con una lógica iterativa, priorizando las funciones que permiten demostrar el flujo principal de negocio.")
    h(d, "3. Objetivos y cumplimiento")
    table(d, ["Objetivo", "Indicador", "Cumplimiento"], [
        ("Catálogo de productos", "Productos con precio, stock y mínimo.", "Cumplido en el MVP con datos demostrativos."),
        ("Registro de ventas", "Total, método de pago, confirmación y descuento de stock.", "Cumplido y verificado con IT-02."),
        ("Control de stock", "Filtro y alerta de productos bajo mínimo.", "Cumplido en Dashboard e Inventario."),
        ("Reportes", "Consulta y exportación de datos.", "Cumplido mediante módulo Reportes y CSV."),
        ("Documentación", "Manual, instalación, técnica, pruebas y sustentación.", "Cumplido con los archivos Word de esta carpeta."),
    ])
    h(d, "4. Producto desarrollado")
    h(d, "4.1 Módulos", 2)
    bullets(d, ["Dashboard: KPIs, gráfico de ventas, actividad reciente y alerta de stock.", "Nueva venta: búsqueda, carrito, cantidades, total y método de pago.", "Inventario: tabla de productos, búsqueda y filtro de stock bajo.", "Productos: alta y consulta del catálogo.", "Proveedores: vista de referencia para futuras compras.", "Reportes: ventas, categorías, movimientos y exportación.", "Configuración: preferencias del sistema y datos base.", "Asistente de bodega: consulta offline demostrativa y contrato Watson documentado."])
    h(d, "4.2 Componentes y recursos entregados", 2)
    table(d, ["Componente", "Ubicación / formato", "Uso"], [
        ("Aplicación", "src/main.jsx y src/styles.css", "Interfaz y lógica del MVP."),
        ("Esquema de datos", "database/schema.sql", "Modelo base para SQL Server."),
        ("Diagramas", "docs/diagramas/", "Arquitectura y modelo de datos."),
        ("Prueba Java", "integration/JavaInventoryIntegrationTest.java", "Verifica venta y actualización de stock."),
        ("Evidencias", "docs/evidencias/", "Capturas desktop y móvil."),
        ("Documentación Word", "docs/word/", "Informes y manuales editables."),
    ])
    h(d, "5. Diseño, validación y pruebas")
    para(d, "La arquitectura separa presentación, estado y persistencia. En el MVP, los datos se guardan en localStorage para facilitar la demostración; el esquema SQL Server, las relaciones y los procedimientos previstos se documentan como base de una siguiente iteración.")
    table(d, ["Prueba", "Resultado", "Evidencia"], [
        ("Registro de venta", "Aprobada", "Venta #1049 registrada; stock de Leche Gloria 1L de 3 a 2 y Gaseosa Inca Kola de 24 a 23."),
        ("Filtro stock bajo", "Aprobada", "Filtro Bajo muestra cinco productos."),
        ("Validación del formulario", "Aprobada", "Guardar deshabilitado cuando faltan campos válidos."),
        ("Responsive móvil", "Aprobada", "Menú móvil, KPIs y gráfico adaptados a 390 px."),
        ("Build", "Aprobada", "pnpm run build finalizado correctamente."),
        ("Integración Java", "Aprobada", "IT-02 OK: venta registrada y stock actualizado."),
    ])
    h(d, "6. Recursos y operación")
    bullets(d, ["Guía de instalación para ejecutar la aplicación en un entorno local.", "Manual de usuario para dashboard, productos, ventas, inventario y reportes.", "Manual técnico con estructura, reglas, persistencia y ruta de evolución.", "Matriz de pruebas y guion para la sustentación.", "Contrato de integración para Watson Assistant, sin almacenar credenciales reales."])
    h(d, "7. Conclusiones")
    numbered(d, ["El MVP demuestra un flujo completo de inventario y ventas con actualización del stock y reportes operativos.", "La organización por entregables permite evidenciar diagnóstico, diseño, construcción, pruebas y documentación.", "La base técnica queda preparada para migrar de almacenamiento local a una arquitectura multiusuario con SQL Server."])
    h(d, "8. Recomendaciones")
    numbered(d, ["Implementar autenticación y control de roles antes de una puesta en producción.", "Realizar pruebas de aceptación con usuarios de la bodega y datos autorizados.", "Configurar respaldos, auditoría de movimientos y la integración Watson en un ambiente seguro."])
    h(d, "9. Glosario")
    table(d, ["Término", "Definición"], [
        ("MVP", "Producto mínimo viable para demostrar el valor principal."),
        ("Stock mínimo", "Cantidad umbral que activa una alerta de reposición."),
        ("Scrum", "Marco ágil para organizar trabajo iterativo."),
        ("Historia de usuario", "Necesidad expresada desde la perspectiva del usuario."),
        ("Watson Assistant", "Servicio conversacional de IBM considerado para una integración posterior."),
        ("SQL Server", "Motor de base de datos previsto para persistencia multiusuario."),
    ])
    h(d, "10. Bibliografía y fuentes internas")
    bullets(d, ["Anexo 4 – Informe de Proyecto para ETI, Plan Nivel 2 para EFSRT III, IV y V.", "Plan de Implementación de EFSRT – ETI 2024.", "Sílabo del curso: EFSRT III (código 14908), periodo 2026.", "Material de clase y anuncio de cronograma publicado por la docente.", "Documentación técnica del proyecto incluida en la carpeta entregable."])
    h(d, "11. Apéndices")
    para(d, "A. Capturas del sistema")
    image(d, "docs/evidencias/EVID_visual_escritorio.png", "E-01: vista general del sistema en escritorio", 6.5)
    image(d, "docs/evidencias/EVID_visual_movil.png", "E-02: vista general del sistema en móvil", 3.2)
    para(d, "B. Pantalla conceptual")
    image(d, "public/design-concept-dashboard.png", "Referencia visual utilizada para orientar el diseño del dashboard", 6.5)
    para(d, "C. Aprobaciones")
    table(d, ["Rol", "Nombre / firma", "Fecha"], [("Equipo", "[Completar]", "[Completar]"), ("Docente", TEACHER, "[Completar]"), ("Organización", "[Completar]", "[Completar]")])
    save(d, "03_Entrega_Final_Informe.docx")


def doc_04():
    d = Document(); cover(d, "Cronograma de entregas y evidencias", "Documento de control del proyecto")
    h(d, "1. Propósito")
    para(d, "Este documento consolida el cronograma del proyecto EFSR14908 y relaciona cada hito con los documentos Word que deben presentarse. Las fechas exactas deben completarse con el archivo de cronograma publicado en el aula virtual.")
    h(d, "2. Cronograma académico de referencia")
    table(d, ["Hito", "Semana / referencia", "Contenido", "Documento Word"], [
        ("Entregable 1", "Próxima semana según anuncio de semana 2", "Capítulos I y II: diagnóstico, objetivos, justificación, alcance y propuesta.", "01_Entregable_1_Informe_Inicial.docx"),
        ("Avance 50%", "Semana 10 según Anexo 4 Nivel 2", "Proyecto al menos 50% y defensa grupal.", "02_Entregable_2_Avance_50.docx"),
        ("Entrega final", "Semana 13 según Anexo 4 Nivel 2", "Proyecto 100% y defensa grupal.", "03_Entrega_Final_Informe.docx"),
        ("Evaluación del sílabo", "Semana 07 según Silabo_del_curso (10).pdf", "Evaluación final / entrega del proyecto.", "Verificar fecha en aula virtual."),
        ("Microcertificaciones", "Semana 08 según Silabo_del_curso (10).pdf", "Actividad posterior indicada en el sílabo.", "Según indicación docente."),
    ])
    h(d, "3. Plan semanal de trabajo")
    table(d, ["Semana", "Actividad", "Evidencia"], [
        ("1", "Formación de grupos y selección del problema.", "Lista de equipo y tema."),
        ("2", "Inicio del proyecto y definición de capítulos I y II.", "Entregable 1 en Word."),
        ("3–5", "Análisis, historias de usuario, alcance, diseño y base de datos.", "Diagramas y backlog."),
        ("6–9", "Construcción del MVP, módulos y pruebas iniciales.", "Aplicación funcional y matriz de pruebas."),
        ("10", "Presentación del avance mínimo 50%.", "Entregable 2 en Word y defensa."),
        ("11–12", "Integración, documentación, validación y correcciones.", "Manuales y recursos."),
        ("13", "Entrega del proyecto al 100% y defensa.", "Informe final en Word y sustentación."),
    ])
    h(d, "4. Lista de verificación antes de entregar")
    bullets(d, ["Completar nombres, integrantes, institución, ubicación, fechas y firmas.", "Revisar que el informe esté en Word editable y conserve tablas, imágenes y títulos.", "Adjuntar código fuente, SQL, diagramas, evidencias, manuales y matriz de pruebas.", "Comprobar que la defensa explique problema, objetivos, producto, resultados y mejoras.", "Verificar el cronograma exacto publicado por la docente, debido a diferencias entre el Anexo 4 y el sílabo consultado."])
    h(d, "5. Observación importante")
    para(d, "Los documentos revisados presentan referencias de semanas distintas: el Anexo 4 Nivel 2 señala 50% en semana 10 y final en semana 13; el sílabo consultado señala evaluación final en semana 7. Para evitar una entrega fuera de plazo, prevalece la fecha específica del aula virtual y del anuncio vigente de la docente.")
    save(d, "04_Cronograma.docx")


def doc_05():
    d = Document(); cover(d, "Manual de usuario", "Guía de operación del sistema Bodega Norte")
    h(d, "1. Presentación")
    para(d, "Este manual explica cómo utilizar el MVP del sistema de inventario y ventas. Está dirigido al administrador o encargado de una bodega y usa un lenguaje operativo, con pasos breves para las tareas frecuentes.")
    h(d, "2. Ingreso y navegación")
    numbered(d, ["Abra la aplicación en el navegador desde la dirección entregada por el equipo.", "Revise el Dashboard para conocer ventas del día, ingresos, productos y alertas.", "Use el menú lateral en escritorio o el botón Abrir menú en móvil.", "Seleccione Inventario, Productos, Nueva venta, Reportes o Configuración según la tarea."])
    h(d, "3. Registrar una venta")
    numbered(d, ["Seleccione Nueva venta.", "Busque un producto por nombre o categoría.", "Agregue el producto al carrito e indique la cantidad.", "Verifique subtotal y total; no ingrese una cantidad superior al stock disponible.", "Seleccione el método de pago: efectivo, Yape, Plin o tarjeta.", "Pulse Cobrar venta y luego Registrar venta.", "Confirme el mensaje de operación registrada y revise que el stock haya disminuido."])
    h(d, "4. Consultar inventario")
    numbered(d, ["Abra Inventario.", "Use el buscador para localizar un producto.", "Use el filtro Todos, Disponible o Bajo.", "Considere que un producto está bajo cuando su stock actual es menor o igual a su stock mínimo.", "Planifique la reposición tomando como referencia el listado y el reporte."])
    h(d, "5. Agregar un producto")
    numbered(d, ["Abra Productos y pulse Agregar producto.", "Complete nombre, categoría, precio, stock y stock mínimo.", "Revise que el precio sea mayor que cero y que las cantidades sean válidas.", "Pulse Guardar producto; si falta un dato, el botón permanecerá deshabilitado."])
    h(d, "6. Revisar reportes")
    bullets(d, ["En Reportes, revise el resumen de ventas, métodos de pago, categorías y movimientos.", "Use Exportar CSV para descargar los datos y abrirlos en Excel.", "Conserve el archivo exportado como respaldo y no comparta datos sensibles sin autorización."])
    h(d, "7. Usar el asistente de bodega")
    numbered(d, ["Pulse el botón Asistente de bodega.", "Seleccione una consulta rápida como Stock bajo o Ventas de hoy.", "Lea la respuesta resumida y valide el detalle en Inventario o Reportes.", "La versión actual es una demostración offline; la integración productiva con Watson está documentada aparte."])
    h(d, "8. Recomendaciones de uso")
    bullets(d, ["Registre la venta inmediatamente después de atender al cliente.", "Revise el stock bajo al inicio y al cierre de la jornada.", "No recargue la página durante una operación sin confirmar si el navegador está procesando la venta.", "Realice exportaciones periódicas hasta disponer de persistencia en servidor."])
    h(d, "9. Errores frecuentes")
    table(d, ["Situación", "Acción"], [
        ("No puedo guardar un producto", "Complete todos los campos y use valores válidos."),
        ("El producto no aparece", "Revise el buscador, categoría y que el catálogo esté cargado."),
        ("No puedo vender la cantidad", "La cantidad supera el stock disponible; reduzca unidades o reponga."),
        ("El reporte parece vacío", "Verifique que existan ventas registradas en el periodo de demostración."),
        ("Necesito conservar los datos", "Use Exportar CSV y siga la guía de migración a SQL Server."),
    ])
    save(d, "05_Manual_Usuario.docx")


def doc_06():
    d = Document(); cover(d, "Guía de instalación y puesta en marcha", "Documento operativo para el entorno local")
    h(d, "1. Alcance")
    para(d, "La guía permite ejecutar el MVP de Bodega Norte en un equipo de desarrollo. La aplicación se entrega como frontend demostrativo y utiliza persistencia local del navegador. El esquema SQL Server se incluye como base de una instalación multiusuario posterior.")
    h(d, "2. Requisitos")
    bullets(d, ["Windows, macOS o Linux con terminal.", "Node.js 20 o superior y pnpm.", "Navegador actualizado: Chrome, Edge o Firefox.", "Java 17 o superior solo para ejecutar la prueba de integración opcional.", "SQL Server solo si se implementa la persistencia de la siguiente fase."])
    h(d, "3. Instalación del frontend")
    numbered(d, ["Copie o clone la carpeta del proyecto en el equipo.", "Abra una terminal en la carpeta EFSR14908_Bodega_Norte.", "Ejecute pnpm install para instalar las dependencias.", "Ejecute pnpm run dev para iniciar el servidor local.", "Abra la URL indicada por Vite, normalmente http://localhost:5173/."])
    h(d, "4. Construcción de producción")
    para(d, "Para validar que la aplicación puede compilarse, ejecute:")
    p = d.add_paragraph(); r = p.add_run("pnpm run build"); r.font.name = "Consolas"; r.font.size = Pt(10); r.font.color.rgb = RGBColor.from_string(BLUE)
    para(d, "El comando debe finalizar sin errores y crear la carpeta dist.")
    h(d, "5. Datos y persistencia")
    para(d, "La demo guarda productos y ventas en localStorage con las claves bodega-norte:v1:products y bodega-norte:v1:sales. Estos datos pertenecen al navegador y equipo donde se ejecuta la demo; no son un respaldo productivo.")
    table(d, ["Fase", "Persistencia", "Acción requerida"], [
        ("MVP demostrativo", "localStorage", "Usar Exportar CSV para respaldar."),
        ("Piloto", "SQL Server local o institucional", "Ejecutar database/schema.sql y crear API segura."),
        ("Producción", "SQL Server administrado", "Autenticación, roles, auditoría, copias y monitoreo."),
    ])
    h(d, "6. Configuración ambiental")
    para(d, "Copie .env.example como .env solo si se agregan servicios externos. Nunca coloque claves reales en el repositorio ni en las capturas de la entrega.")
    table(d, ["Variable sugerida", "Uso"], [
        ("VITE_API_BASE_URL", "URL del backend futuro."),
        ("WATSON_ASSISTANT_ID", "Identificador del asistente."),
        ("WATSON_APIKEY", "Clave privada; usar solo en servidor."),
        ("WATSON_SERVICE_URL", "Endpoint del servicio conversacional."),
    ])
    h(d, "7. Ejecución de prueba Java")
    para(d, "La prueba de integración se encuentra en integration/JavaInventoryIntegrationTest.java. Compile y ejecútela con el JDK disponible, según las instrucciones de integration/README.md. El resultado esperado es: IT-02 OK: venta registrada y stock actualizado.")
    h(d, "8. Solución de problemas")
    table(d, ["Problema", "Solución"], [
        ("pnpm no reconocido", "Instale Node.js y habilite pnpm mediante Corepack o instalación oficial."),
        ("Puerto ocupado", "Cierre otro proceso o inicie Vite con un puerto disponible."),
        ("La pantalla no actualiza", "Recargue la aplicación y revise la consola del navegador."),
        ("Se perdieron datos del demo", "Verifique el mismo navegador y use respaldos CSV; localStorage no reemplaza una base de datos."),
    ])
    save(d, "06_Guia_Instalacion.docx")


def doc_07():
    d = Document(); cover(d, "Manual técnico", "Arquitectura, componentes y evolución de Bodega Norte")
    h(d, "1. Arquitectura actual")
    para(d, "El sistema se entrega como SPA construida con React y Vite. La aplicación concentra la presentación y las reglas del MVP en el cliente, con almacenamiento local para facilitar una demostración autónoma. La arquitectura está preparada para extraer la persistencia y las integraciones hacia servicios backend.")
    table(d, ["Capa", "Implementación", "Responsabilidad"], [
        ("Presentación", "React + CSS", "Vistas, componentes, navegación responsive y estados visuales."),
        ("Dominio", "Funciones del frontend", "Cálculo de totales, validación, stock, reportes y filtros."),
        ("Persistencia demo", "localStorage", "Conservar productos y ventas del navegador."),
        ("Persistencia futura", "SQL Server", "Modelo relacional multiusuario mediante API segura."),
        ("Integración futura", "Watson Assistant", "Consultas conversacionales de inventario y ventas."),
    ])
    h(d, "2. Estructura del proyecto")
    table(d, ["Ruta", "Contenido"], [
        ("src/main.jsx", "Composición de la aplicación, vistas y lógica del MVP."),
        ("src/styles.css", "Sistema visual, layout, responsive y estados."),
        ("database/schema.sql", "Tablas, relaciones, índices y procedimientos previstos."),
        ("integration/", "Prueba de integración Java y guía asociada."),
        ("docs/diagramas/", "Arquitectura draw.io y modelo de datos SVG."),
        ("docs/word/", "Informes y manuales editables para la entrega académica."),
    ])
    h(d, "3. Modelo de datos previsto")
    table(d, ["Entidad", "Campos principales", "Relación"], [
        ("products", "id, name, category, price, stock, min_stock", "Un producto participa en detalles de venta."),
        ("sales", "id, sold_at, payment_method, total", "Una venta contiene uno o más detalles."),
        ("sale_items", "id, sale_id, product_id, quantity, unit_price", "Relaciona ventas y productos."),
        ("stock_movements", "id, product_id, type, quantity, reason", "Audita entradas, salidas y ajustes."),
        ("suppliers", "id, name, contact", "Base para módulo de compras futuro."),
    ])
    h(d, "4. Reglas de negocio")
    bullets(d, ["Una venta no debe superar el stock disponible.", "El total de una venta es la suma de cantidad por precio unitario.", "Stock bajo cuando stock actual es menor o igual a stock mínimo.", "El registro de una venta descuenta unidades y guarda el método de pago.", "Los datos de entrada deben validarse antes de persistir.", "Las claves de Watson deben permanecer fuera del cliente y del repositorio."])
    h(d, "5. Migración a SQL Server")
    numbered(d, ["Crear la base según database/schema.sql.", "Implementar API con endpoints autenticados para productos, ventas, stock y reportes.", "Mover las operaciones de cálculo de stock al servidor dentro de una transacción.", "Agregar índices para búsqueda por nombre, categoría y fecha.", "Implementar auditoría de movimientos y control de concurrencia.", "Cambiar el frontend para consumir VITE_API_BASE_URL y manejar estados de carga y error."])
    h(d, "6. Integración Watson Assistant")
    para(d, "La integración documentada separa intents, entidades, respuestas y contrato de backend. El navegador no debe llamar directamente al servicio con una API key; el backend debe autenticar, filtrar la consulta y devolver solo información permitida para el usuario.")
    h(d, "7. Calidad y seguridad")
    bullets(d, ["Ejecutar pnpm run build antes de cada entrega.", "Mantener pruebas del flujo de venta y actualización de stock.", "Revisar accesibilidad de botones, formularios y navegación móvil.", "No subir .env, tokens, credenciales ni datos reales de clientes.", "Respaldar documentación y exportaciones fuera del navegador."])
    h(d, "8. Evidencia visual")
    image(d, "docs/evidencias/EVID_visual_escritorio.png", "Vista validada del dashboard y módulos principales", 6.5)
    save(d, "07_Manual_Tecnico.docx")


def doc_08():
    d = Document(); cover(d, "Matriz de pruebas y validación", "Evidencias funcionales, técnicas y de usabilidad")
    h(d, "1. Criterios de prueba")
    para(d, "La matriz cubre los flujos principales del MVP, la validación de formularios, la actualización de existencias, la adaptación responsive, la compilación y la prueba de integración. Cada resultado debe acompañarse de captura o registro de ejecución cuando se presente el informe.")
    h(d, "2. Casos de prueba")
    rows = [
        ("IT-01", "Dashboard", "Abrir la aplicación", "Se muestran KPIs, gráfico, actividad y alerta", "Aprobado"),
        ("IT-02", "Ventas", "Vender 1 Leche y 1 Gaseosa", "Venta registrada y stock actualizado", "Aprobado"),
        ("IT-03", "Ventas", "Intentar vender más stock", "Operación bloqueada o validada", "Aprobado"),
        ("IT-04", "Inventario", "Usar filtro Bajo", "Lista solo productos bajo mínimo", "Aprobado"),
        ("IT-05", "Inventario", "Buscar por nombre", "Coincidencias relevantes", "Aprobado"),
        ("IT-06", "Productos", "Abrir modal Agregar producto", "Campos visibles y editables", "Aprobado"),
        ("IT-07", "Productos", "Guardar sin datos completos", "Botón deshabilitado / validación", "Aprobado"),
        ("IT-08", "Reportes", "Abrir Reportes", "Resumen de ventas y stock", "Aprobado"),
        ("IT-09", "Reportes", "Exportar CSV", "Se descarga archivo editable", "Aprobado"),
        ("IT-10", "Responsive", "Abrir con ancho 390 px", "Menú y tarjetas adaptadas", "Aprobado"),
        ("IT-11", "Asistente", "Consultar Stock bajo", "Respuesta con productos y cantidad", "Aprobado"),
        ("IT-12", "Build", "Ejecutar pnpm run build", "Compilación sin errores", "Aprobado"),
        ("IT-13", "Java", "Ejecutar integración de venta", "IT-02 OK", "Aprobado"),
        ("IT-14", "Documentación", "Revisar archivos Word", "Editables, con títulos y tablas", "Pendiente firma"),
    ]
    table(d, ["ID", "Módulo", "Entrada / acción", "Resultado esperado", "Resultado"], rows)
    h(d, "3. Evidencias que deben adjuntarse")
    bullets(d, ["Captura del Dashboard en escritorio.", "Captura del Dashboard en móvil.", "Captura del mensaje de venta registrada.", "Captura del stock antes y después de una venta.", "Captura del filtro de stock bajo.", "Registro de build y prueba Java.", "Firma o validación del usuario responsable de la bodega, si corresponde."])
    h(d, "4. Registro de ejecución")
    table(d, ["Fecha", "Ejecutor", "Ambiente / navegador", "Observaciones"], [("[Completar]", "[Completar]", "[Completar]", "[Completar]"), ("[Completar]", "[Completar]", "[Completar]", "[Completar]")])
    h(d, "5. Aceptación")
    table(d, ["Rol", "Nombre / firma", "Fecha"], [("Equipo", "[Completar]", "[Completar]"), ("Docente", TEACHER, "[Completar]"), ("Usuario validador", "[Completar]", "[Completar]")])
    save(d, "08_Matriz_Pruebas.docx")


def doc_09():
    d = Document(); cover(d, "Guion de sustentación", "Defensa grupal del proyecto Bodega Norte")
    h(d, "1. Objetivo de la exposición")
    para(d, "Presentar en forma clara el problema, la solución, el proceso de desarrollo, las evidencias y la viabilidad del sistema. La exposición debe mostrar el producto funcionando y explicar qué se entrega en cada hito.")
    h(d, "2. Distribución sugerida")
    table(d, ["Bloque", "Tiempo sugerido", "Responsable", "Contenido"], [
        ("Contexto", "2 min", "Integrante 1", "Bodega, problema y oportunidad."),
        ("Propuesta", "2 min", "Integrante 1", "Objetivos, alcance y beneficiarios."),
        ("Diseño", "3 min", "Integrante 2", "Scrum, historias, arquitectura y datos."),
        ("Demo", "5 min", "Integrante 3", "Dashboard, productos, venta, stock y reportes."),
        ("Pruebas", "2 min", "Integrante 4", "Casos, resultados y evidencias."),
        ("Cierre", "2 min", "Todo el equipo", "Conclusiones, mejoras y preguntas."),
    ])
    h(d, "3. Guion de demostración")
    numbered(d, ["Mostrar el Dashboard y explicar los cuatro indicadores principales.", "Abrir Inventario y aplicar el filtro Bajo para mostrar las alertas.", "Abrir Nueva venta, seleccionar Leche Gloria 1L y Gaseosa Inca Kola, elegir Yape y registrar la venta.", "Volver al inventario para mostrar que el stock se actualizó.", "Abrir Reportes y explicar el resumen y la exportación CSV.", "Abrir Asistente de bodega y consultar Stock bajo; aclarar que la conexión Watson queda preparada para la siguiente fase."])
    h(d, "4. Mensajes clave")
    bullets(d, ["El sistema resuelve un problema operativo concreto y medible.", "El flujo principal de negocio está implementado y probado.", "La interfaz es responsive y permite una operación rápida.", "El MVP es demostrable, pero la puesta en producción requiere backend, autenticación y base de datos.", "La documentación permite mantener y ampliar el proyecto."])
    h(d, "5. Preguntas previsibles")
    table(d, ["Pregunta", "Respuesta sugerida"], [
        ("¿Por qué localStorage?", "Permite demostrar el flujo sin depender de un servidor; SQL Server queda preparado para la fase multiusuario."),
        ("¿Cómo se evita vender sin stock?", "La cantidad se valida contra el stock disponible antes de confirmar."),
        ("¿Qué hace el asistente?", "La demo responde consultas de stock; el contrato Watson define la integración segura futura."),
        ("¿Cómo se respalda la información?", "Actualmente mediante exportación CSV; en producción se usarán copias de SQL Server."),
        ("¿Cómo se probó?", "Con casos funcionales, responsive, build y una integración Java que verifica venta y stock."),
    ])
    h(d, "6. Cierre sugerido")
    para(d, "Bodega Norte transforma el registro manual de productos y ventas en una experiencia digital simple, verificable y escalable. El proyecto cumple la entrega del MVP y deja documentadas las siguientes mejoras para una implementación institucional.")
    h(d, "7. Datos para completar")
    table(d, ["Dato", "Completar"], [("Integrantes", "[Completar]"), ("Orden de exposición", "[Completar]"), ("Duración asignada", "[Completar]"), ("Fecha y aula", "[Completar]")])
    save(d, "09_Guion_Sustentacion.docx")


def doc_10():
    d = Document(); cover(d, "Especificación de integración con Watson Assistant", "Contrato funcional y técnico para una fase posterior")
    h(d, "1. Propósito")
    para(d, "Este documento define cómo incorporar un asistente conversacional al sistema Bodega Norte. La versión actual incluye un asistente offline demostrativo; este diseño permite conectar Watson Assistant sin exponer credenciales ni reglas internas en el navegador.")
    h(d, "2. Intents propuestos")
    table(d, ["Intent", "Ejemplos", "Respuesta esperada"], [
        ("stock_bajo", "¿Qué productos están bajos?", "Lista de productos cuyo stock actual está en o debajo del mínimo."),
        ("ventas_hoy", "¿Cuánto vendimos hoy?", "Total de ventas e ingresos del día."),
        ("buscar_producto", "Busca arroz", "Producto, precio y disponibilidad."),
        ("resumen_inventario", "Dame un resumen del inventario", "Cantidad de productos, valorización referencial y alertas."),
        ("ayuda_sistema", "¿Cómo registro una venta?", "Pasos resumidos para completar la tarea."),
    ])
    h(d, "3. Entidades")
    table(d, ["Entidad", "Valores / ejemplo", "Uso"], [
        ("producto", "Leche Gloria 1L", "Buscar un artículo específico."),
        ("categoria", "Lácteos, bebidas, abarrotes", "Filtrar catálogo."),
        ("periodo", "hoy, semana, mes", "Delimitar reportes."),
        ("metodo_pago", "efectivo, Yape, Plin, tarjeta", "Consultar ventas por forma de pago."),
    ])
    h(d, "4. Contrato de backend")
    table(d, ["Método", "Ruta", "Propósito"], [
        ("GET", "/api/inventory/low-stock", "Obtener productos con stock bajo."),
        ("GET", "/api/reports/sales?period=today", "Obtener resumen de ventas."),
        ("GET", "/api/products/search?q=...", "Buscar producto."),
        ("GET", "/api/inventory/summary", "Obtener resumen de inventario."),
    ])
    h(d, "5. Flujo seguro")
    numbered(d, ["El usuario envía una pregunta desde la interfaz.", "El frontend envía la consulta al backend de Bodega Norte.", "El backend valida usuario, permisos y parámetros.", "El backend llama a Watson Assistant usando credenciales protegidas.", "El backend consulta solo los datos autorizados y forma una respuesta segura.", "El frontend presenta la respuesta y ofrece un enlace al módulo correspondiente."])
    h(d, "6. Reglas de seguridad")
    bullets(d, ["Nunca colocar WATSON_APIKEY en el código cliente ni en una captura.", "Usar variables de entorno en el backend y rotar credenciales.", "Limitar respuestas a datos necesarios para el rol del usuario.", "Registrar errores sin guardar preguntas que contengan información sensible.", "Aplicar rate limiting y validar entradas antes de consultar la base de datos."])
    h(d, "7. Criterios de aceptación")
    table(d, ["Criterio", "Evidencia"], [
        ("El intent stock_bajo reconoce variaciones de la pregunta.", "Pruebas conversacionales documentadas."),
        ("La respuesta coincide con el inventario autorizado.", "Comparación con /api/inventory/low-stock."),
        ("No se exponen credenciales.", "Revisión de variables y repositorio."),
        ("El asistente funciona sin romper la venta.", "Prueba de regresión IT-02."),
    ])
    h(d, "8. Estado actual")
    para(d, "Estado: especificación preparada; integración productiva pendiente de credenciales, configuración de Watson, backend y validación institucional. La aplicación conserva una respuesta offline para que la demostración del proyecto sea autónoma.")
    save(d, "10_Integracion_Watson_Assistant.docx")


def main():
    for f in OUT.glob("*.docx"):
        f.unlink()
    doc_01(); doc_02(); doc_03(); doc_04(); doc_05(); doc_06(); doc_07(); doc_08(); doc_09(); doc_10()
    print(f"Created {len(list(OUT.glob('*.docx')))} Word documents in {OUT}")


if __name__ == "__main__":
    main()
