from pathlib import Path
from html import escape
from PIL import Image,ImageDraw,ImageFont
import xml.etree.ElementTree as ET
root=Path(__file__).resolve().parent/'docs/diagramas';root.mkdir(parents=True,exist_ok=True)
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',22);bold=ImageFont.truetype('C:/Windows/Fonts/arialbd.ttf',25)
def diagram(name,title,nodes,edges,size,notes):
    w,h=size;im=Image.new('RGB',size,'white');draw=ImageDraw.Draw(im);draw.text((30,20),title,font=bold,fill='black')
    svg=[f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}"><rect width="100%" height="100%" fill="white"/><defs><marker id="arrow" markerWidth="10" markerHeight="10" refX="9" refY="5" orient="auto"><path d="M0,0 L10,5 L0,10" fill="black"/></marker></defs>',f'<text x="30" y="44" font-family="Arial" font-size="25" font-weight="bold">{escape(title)}</text>']
    mx=ET.Element('mxfile',host='app.diagrams.net');page=ET.SubElement(mx,'diagram',name=title);model=ET.SubElement(page,'mxGraphModel',pageWidth=str(w),pageHeight=str(h));rt=ET.SubElement(model,'root');ET.SubElement(rt,'mxCell',id='0');ET.SubElement(rt,'mxCell',id='1',parent='0')
    for key,x,y,bw,bh,head,lines in nodes:
        draw.rectangle((x,y,x+bw,y+bh),outline='black',width=2);draw.line((x,y+44,x+bw,y+44),fill='black',width=2)
        draw.text((x+12,y+8),head,font=bold,fill='black')
        svg.extend([f'<rect x="{x}" y="{y}" width="{bw}" height="{bh}" fill="white" stroke="black" stroke-width="2"/>',f'<line x1="{x}" y1="{y+44}" x2="{x+bw}" y2="{y+44}" stroke="black"/>',f'<text x="{x+12}" y="{y+31}" font-family="Arial" font-size="25" font-weight="bold">{escape(head)}</text>'])
        for i,line in enumerate(lines):
            draw.text((x+12,y+54+i*27),line,font=font,fill='black');svg.append(f'<text x="{x+12}" y="{y+75+i*27}" font-family="Arial" font-size="22">{escape(line)}</text>')
        cell=ET.SubElement(rt,'mxCell',id=key,value=head+'\n'+'\n'.join(lines),style='rounded=0;whiteSpace=wrap;html=0;fillColor=#ffffff;strokeColor=#000000;fontColor=#000000;fontSize=16;',vertex='1',parent='1');ET.SubElement(cell,'mxGeometry',x=str(x),y=str(y),width=str(bw),height=str(bh),attrib={'as':'geometry'})
    for n,(source,target,points) in enumerate(edges):
        draw.line(points,fill='black',width=2);end=points[-1];prev=points[-2];dx=end[0]-prev[0];dy=end[1]-prev[1]
        import math
        length=math.hypot(dx,dy);ux,uy=dx/length,dy/length;triangle=[end,(end[0]-12*ux+5*uy,end[1]-12*uy-5*ux),(end[0]-12*ux-5*uy,end[1]-12*uy+5*ux)];draw.polygon(triangle,fill='black')
        svg.append('<polyline points="'+' '.join(f'{a},{b}' for a,b in points)+'" fill="none" stroke="black" stroke-width="2" marker-end="url(#arrow)"/>')
        cell=ET.SubElement(rt,'mxCell',id=f'edge{n}',source=source,target=target,edge='1',parent='1',style='edgeStyle=orthogonalEdgeStyle;endArrow=block;strokeColor=#000000;');ET.SubElement(cell,'mxGeometry',relative='1',attrib={'as':'geometry'})
    for i,line in enumerate(notes):
        y=h-65+i*27;draw.text((30,y),line,font=font,fill='black');svg.append(f'<text x="30" y="{y+20}" font-family="Arial" font-size="22">{escape(line)}</text>')
    svg.append('</svg>');im.save(root/f'{name}_bw.png');im.save(root/f'{name}.png');(root/f'{name}.svg').write_text('\n'.join(svg),encoding='utf-8');(root/f'{name}_bw.svg').write_text('\n'.join(svg),encoding='utf-8');ET.ElementTree(mx).write(root/f'{name}.drawio',encoding='utf-8',xml_declaration=True)
diagram('modelo_datos','Modelo PostgreSQL Bodega Norte',[
 ('categories',30,90,330,180,'categories',['PK category_id','name']),('products',435,90,330,180,'products',['PK product_id; code','FK category_id; supplier_id','stock; min_stock','cost; sale_price; version']),('movements',840,90,330,180,'stock_movements',['PK movement_id','FK product_id','quantity; balance; type','reason; actor_id; created_at']),
 ('suppliers',30,365,330,170,'suppliers',['PK supplier_id','name; contact_name; phone']),('sales',435,365,330,170,'sales',['PK sale_id; sale_number','request_id; sold_at','total; status; payment_method','created_by; cancel_reason']),('items',840,365,330,170,'sale_items',['PK sale_item_id','FK sale_id; product_id','quantity; unit_price','unit_cost; line_total']),
 ('members',30,650,330,170,'members',['PK user_id = auth.users.id','email; role; is_active']),('settings',435,650,330,170,'settings',['PK id (solo valor 1)','name; ruc; address','phone; stock_alerts']),('audit',840,650,330,170,'audit_log',['PK audit_id','action; entity; entity_id','actor_id; actor_email','created_at'])],
 [('categories','products',[(360,155),(435,155)]),('suppliers','products',[(360,405),(390,405),(390,230),(435,230)]),('products','movements',[(765,155),(840,155)]),('products','items',[(765,230),(795,230),(795,415),(840,415)]),('sales','items',[(765,470),(840,470)])],(1200,925),['Flechas: referencias de clave foránea. Campos principales resumidos.','members referencia auth.users, tabla externa al esquema privado bodega_norte.'])
diagram('arquitectura','Arquitectura publicada Bodega Norte',[
 ('view',25,90,330,215,'Vista React',['GitHub Pages','Sesión Supabase Auth','Ventas y consultas','Ayuda local independiente']),('mvc',435,90,330,215,'Backend MVC',['Render y ASP.NET Core 8','Controladores y servicios','Roles desde members','Repositorio Npgsql']),('db',845,90,330,215,'Supabase',['PostgreSQL privado','9 tablas con RLS','TLS VerifyFull','Auth valida la identidad']),('ibm',435,415,740,155,'IBM Watson Assistant',['Assistant builder de watsonx Orchestrate','IAM y API v2 con clave privada en servidor','8 acciones en español; Trial gratuito de 30 días'])],
 [('view','mvc',[(355,190),(435,190)]),('mvc','db',[(765,190),(845,190)]),('mvc','ibm',[(600,305),(600,415)])],(1200,690),['Las respuestas IBM orientan; no ejecutan ventas ni reciben los registros de la base.','El frontend y la demo no contienen la conexión PostgreSQL ni la clave privada IBM.'])
print('Diagramas actuales creados en PNG SVG y drawio.')
