const { chromium } = await import(process.env.BODEGA_PLAYWRIGHT_MODULE || 'playwright');
const testUrl = process.env.BODEGA_TEST_URL || 'http://127.0.0.1:5360/';
if(!['127.0.0.1','localhost'].includes(new URL(testUrl).hostname)) throw new Error('La prueba de interfaz requiere un servidor local aislado en modo demo.');
import fs from 'node:fs/promises';
import path from 'node:path';
const root=process.cwd(),out=path.join(root,'output','playwright-final');
await fs.mkdir(out,{recursive:true});
const browser=await chromium.launch({channel:'chrome',headless:true});
const checks=[],errors=[],warnings=[],shots=[];
const context=await browser.newContext({viewport:{width:1365,height:900},acceptDownloads:true});
const page=await context.newPage();
page.on('pageerror',error=>errors.push(error.message));
page.on('requestfailed',request=>errors.push(request.url().split('?')[0]+' '+request.failure()?.errorText));
page.on('console',message=>{if(message.type()==='error')errors.push(message.text());if(message.type()==='warning')warnings.push(message.text());});
async function check(id,description,action){try{await action();checks.push({id,description,status:'aprobado'});console.log(id+' OK: '+description);}catch(error){checks.push({id,description,status:'falló',detail:error.message});throw error;}}
async function shot(filename,caption){await page.screenshot({path:path.join(out,filename),fullPage:false});shots.push({file:filename,caption});}
async function nav(name,title){await page.getByRole('navigation',{name:'Navegación principal'}).getByRole('button',{name:new RegExp('^'+name)}).click();await page.getByRole('heading',{name:title,exact:true}).waitFor();}
function ensure(condition,message){if(!condition)throw new Error(message);}
let finished=false;
try{
  await check('UI-01','Inicio carga con catálogo y datos de demostración',async()=>{
    await page.goto(testUrl);await page.getByRole('heading',{name:'Hola, administrador'}).waitFor();
    ensure((await page.title()).includes('Bodega Norte'),'Título inesperado.');
    ensure((await page.locator('body').innerText()).includes('datos ficticios'),'Falta el contexto demo.');
    await shot('01_inicio.png','Panel en escritorio con datos de demostración');
  });
  await check('UI-02','Inventario filtra stock y descarga CSV',async()=>{
    await nav('Inventario','Inventario');await page.getByLabel('Filtrar por stock').selectOption('Bajo');
    ensure(await page.getByRole('table').getByRole('row').count()===6,'Filtro de stock bajo incorrecto.');
    await shot('02_inventario.png','Filtro de stock bajo en la demostración local');
    await page.getByLabel('Filtrar por stock').selectOption('Todos');
    const promise=page.waitForEvent('download');await page.getByRole('button',{name:'Exportar CSV',exact:true}).click();
    const download=await promise;await download.saveAs(path.join(out,'inventario_verificado.csv'));
    const csv=await fs.readFile(path.join(out,'inventario_verificado.csv'),'utf8');
    ensure(csv.startsWith('\ufeff')&&csv.includes('P-001')&&csv.split('\r\n').length===13,'CSV incompleto.');
  });
  await check('UI-03','Producto puede crearse editarse y archivarse',async()=>{
    await page.getByRole('button',{name:'Agregar producto',exact:true}).click();
    let dialog=page.getByRole('dialog');await dialog.getByLabel('Nombre del producto').fill('Producto revisión EFSRT');
    await dialog.getByLabel('Stock inicial').fill('5');await dialog.getByLabel('Stock mínimo').fill('2');
    await dialog.getByLabel('Costo unitario').fill('1');await dialog.getByLabel('Precio de venta').fill('2');
    await dialog.getByRole('button',{name:/Guardar producto/}).click();await dialog.waitFor({state:'hidden'});
    await page.getByRole('button',{name:'Editar Producto revisión EFSRT',exact:true}).click();
    dialog=page.getByRole('dialog');await dialog.getByLabel('Nombre del producto').fill('Producto revisión actualizado');
    await dialog.getByRole('button',{name:/Guardar producto/}).click();await dialog.waitFor({state:'hidden'});
    await page.getByRole('button',{name:'Eliminar Producto revisión actualizado',exact:true}).click();
    await page.getByRole('dialog').getByRole('button',{name:'Confirmar eliminación',exact:true}).click();
    await page.getByRole('dialog').waitFor({state:'hidden'});
    ensure(await page.getByRole('button',{name:'Editar Producto revisión actualizado',exact:true}).count()===0,'El producto sigue activo.');
  });
  await check('UI-04','Venta con Yape genera constancia y registra historial',async()=>{
    await nav('Nueva venta','Nueva venta');
    await page.getByRole('button',{name:'Agregar Leche Gloria 1L',exact:true}).click();
    await page.getByRole('button',{name:'Agregar Gaseosa Inca Kola 1.5L',exact:true}).click();
    await shot('03_venta.png','Preparación de venta de demostración por S/ 13.00');
    await page.getByRole('button',{name:'Cobrar venta',exact:true}).click();
    const dialog=page.getByRole('dialog');await dialog.getByLabel('Cliente o referencia opcional').fill('Revisión EFSRT Grupo 10');
    await dialog.getByRole('button',{name:'Yape',exact:true}).click();await dialog.getByRole('button',{name:'Registrar venta',exact:true}).click();
    await page.getByRole('heading',{name:'Constancia interna de venta',exact:true}).waitFor();
    ensure(/Total:\s*S\/\s*13\.00\b/.test(await page.getByRole('dialog').innerText()),'Total incorrecto en constancia.');
    await shot('04_constancia.png','Constancia interna de la venta de demostración');
    await page.getByRole('button',{name:'Cerrar diálogo',exact:true}).click();
    await nav('Historial de ventas','Historial de ventas');
    await page.getByRole('row').filter({hasText:'Revisión EFSRT Grupo 10'}).waitFor();
    await shot('05_historial.png','Venta registrada en el historial de la demostración');
  });
  await check('UI-05','Anulación devuelve existencias y queda trazada',async()=>{
    const row=page.getByRole('row').filter({hasText:'Revisión EFSRT Grupo 10'});
    await row.getByRole('button',{name:'Anular',exact:true}).click();
    await page.getByRole('dialog').getByLabel('Motivo',{exact:true}).fill('Verificación técnica en demo local');
    await page.getByRole('dialog').getByRole('button',{name:'Confirmar',exact:true}).click();
    await page.getByRole('dialog').waitFor({state:'hidden'});await row.getByText('Anulada',{exact:true}).waitFor();
    await nav('Movimientos de stock','Movimientos de stock');
    ensure((await page.getByRole('table').innerText()).includes('Anulación'),'No hay devolución de existencias.');
    await shot('06_movimientos.png','Movimientos de venta y devolución por anulación en demo');
  });
  await check('UI-06','Reportes muestran períodos reales y un día sin ventas da cero',async()=>{
    await nav('Reportes','Reportes');await shot('07_reportes.png','Reporte calculado sobre ventas activas de demostración');
    await page.getByLabel('Desde',{exact:true}).fill('2025-01-01');await page.getByLabel('Hasta',{exact:true}).fill('2025-01-01');
    await page.getByText('0 ventas activas',{exact:true}).waitFor();
    ensure(/S\/\s*0\.00\b/.test(await page.locator('.report-summary').innerText()),'El período vacío mantiene montos de muestra.');
  });
  await check('UI-07','Proveedores y permisos están disponibles al administrador',async()=>{
    await nav('Proveedores','Proveedores');await shot('08_proveedores.png','Proveedores y contactos ficticios del prototipo');
    await nav('Configuración','Configuración');await page.getByRole('button',{name:'Usuarios y permisos',exact:true}).click();
    await page.getByLabel('Correo de la cuenta existente').fill('cajero.qa@example.invalid');
    await page.getByRole('button',{name:'Habilitar acceso',exact:true}).click();
    await page.getByRole('cell',{name:'cajero.qa@example.invalid',exact:true}).waitFor();
    await shot('09_permisos.png','Permisos de demostración para una cuenta ficticia');
  });
  await check('UI-08','Respaldo descargado y recuperación validada desde Configuración',async()=>{
    await page.getByRole('button',{name:'Respaldo de datos',exact:true}).click();
    const promise=page.waitForEvent('download');await page.getByRole('button',{name:'Descargar respaldo JSON',exact:true}).click();
    const download=await promise,backupPath=path.join(out,'respaldo_demo_verificado.json');await download.saveAs(backupPath);
    const backup=JSON.parse(await fs.readFile(backupPath,'utf8'));ensure(backup.format==='demo-v2','Formato de respaldo incorrecto.');
    await page.getByRole('button',{name:'Datos de la bodega',exact:true}).click();await page.getByLabel('Nombre comercial').fill('Bodega temporal QA');
    await page.getByRole('button',{name:'Guardar cambios',exact:true}).click();
    await page.getByText('Configuración guardada.',{exact:true}).waitFor();
    await page.getByRole('button',{name:'Respaldo de datos',exact:true}).click();
    await page.getByLabel('Seleccionar respaldo local').setInputFiles(backupPath);
    await page.getByLabel('Escribe RESTAURAR para confirmar').waitFor();
    await shot('10_recuperacion.png','Validación del respaldo y confirmación antes de recuperar la demo');
    await page.getByLabel('Escribe RESTAURAR para confirmar').fill('RESTAURAR');
    await page.getByRole('button',{name:'Restaurar copia local',exact:true}).click();
    await page.getByText('Copia local recuperada.',{exact:true}).waitFor();
    await page.getByRole('button',{name:'Datos de la bodega',exact:true}).click();
    ensure(await page.getByLabel('Nombre comercial').inputValue()==='Bodega Norte','La configuración no se recuperó.');
    await page.getByRole('button',{name:'Auditoría',exact:true}).click();
    await page.getByText('restore_backup',{exact:true}).waitFor();
    await shot('11_auditoria.png','Auditoría de recuperación de datos en la demostración');
  });
  await check('UI-09','Cajero carece de acciones administrativas',async()=>{
    await page.getByLabel('Simular rol').selectOption('cashier');
    await nav('Inventario','Inventario');
    ensure(await page.getByRole('button',{name:'Agregar producto',exact:true}).count()===0,'Cajero tiene gestión de productos.');
    ensure(await page.getByRole('columnheader',{name:'Acciones',exact:true}).count()===0,'Cajero tiene acciones administrativas.');
    await shot('12_cajero.png','Vista de inventario con permisos simulados de cajero');
  });
  await check('UI-10','Vista móvil y menú sin desbordamiento horizontal',async()=>{
    const mobile=await browser.newContext({viewport:{width:390,height:844}}),p=await mobile.newPage();
    p.on('pageerror',error=>errors.push(error.message));await p.goto(testUrl);
    await p.getByRole('heading',{name:'Hola, administrador'}).waitFor();
    const bounds=await p.evaluate(()=>({width:document.documentElement.clientWidth,scroll:document.documentElement.scrollWidth}));
    ensure(bounds.scroll<=bounds.width+1,'Desbordamiento móvil: '+JSON.stringify(bounds));
    await p.screenshot({path:path.join(out,'13_movil.png'),fullPage:false});shots.push({file:'13_movil.png',caption:'Panel móvil en viewport de 390 por 844 píxeles'});
    await p.getByRole('button',{name:'Abrir menú',exact:true}).click();
    await p.getByRole('navigation',{name:'Navegación principal'}).getByRole('button',{name:'Nueva venta',exact:true}).click();
    await p.getByRole('heading',{name:'Nueva venta',exact:true}).waitFor();await mobile.close();
  });
  await check('UI-11','Ayuda local identifica su proveedor y responde stock sin IBM',async()=>{
    await page.getByRole('button',{name:'Asistente',exact:true}).click();
    await page.getByLabel('Pregunta al asistente').fill('stock bajo');
    await page.getByRole('button',{name:'Enviar',exact:true}).click();
    await page.getByText(/productos requieren reposición:/).waitFor();
    ensure(await page.getByRole('option',{name:'IBM Watson para ayuda operativa'}).count()===0,'La demo simula indebidamente conexión IBM.');
    await shot('14_ayuda_local.png','Ayuda local en demostración; la respuesta identifica su origen');
    await page.getByRole('button',{name:'Cerrar diálogo',exact:true}).click();
  });
  ensure(errors.length===0,'Errores de aplicación: '+errors.join('; '));
  ensure(warnings.length===0,'Advertencias de aplicación: '+warnings.join('; '));
  finished=true;
}finally{
  if(!finished)await page.screenshot({path:path.join(out,'failure.png'),fullPage:false}).catch(()=>{});
  await fs.writeFile(path.join(out,'validacion_interfaz.json'),JSON.stringify({
    date:'2026-10-11',timeZone:'America/Lima',url:testUrl,mode:'Demostración local aislada',
    browser:'Google Chrome con Playwright bundled; Browser skill no disponible',
    desktop:{width:1365,height:900},mobile:{width:390,height:844},allPassed:finished,
    checks,errors,warnings,screenshots:shots},null,2));
  await browser.close();
}
