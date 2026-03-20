import csv
import os

path = "negocios_raw.csv"

with open("negocio.sql", "w", encoding="utf-8") as writer:
    with open(path, 'r', encoding='utf-8') as file:
        reader = csv.DictReader(file)
        lista_zona = []
        lista_region = []
        lista_estados = []
        lista_municipios = []
        lista_categoria = []
        lista_giro = []
        lista_contacto = []
        lista_representante = []
        lista_negocio = []
        
        total_negocios = 0
        for row in reader:
            if not row["zona_economica"] in lista_zona:
                lista_zona.append(row['zona_economica'])
                print(f'insert into zona(id_zona,nombre) values ({lista_zona.index(row["zona_economica"]) + 1}, "{row["zona_economica"]}");', file=writer)
            row["zona_economica"] = lista_zona.index(row["zona_economica"]) + 1
            
            if not row["region"] in lista_region:
                lista_region.append(row['region'])
                print(f'insert into region(id_region,nombre, id_zona) values ({lista_region.index(row["region"]) + 1}, "{row["region"]}", {row["zona_economica"]});', file=writer)
            row["region"] = lista_region.index(row["region"]) + 1
            
            if not row["estado"] in lista_estados:
                #index +=1
                lista_estados.append( row['estado'])
                print(f'insert into estado(id_estado, nombre, id_region) Values({lista_estados.index(row["estado"]) + 1}, "{row["estado"]}", {row["region"]});', file=writer)
            row["estado"] = lista_estados.index(row["estado"]) + 1
            
            if not row["municipio"] in lista_municipios:
                lista_municipios.append(row['municipio'])
                print(f'insert into municipio(id_municipio, nombre, id_estado) values ({lista_municipios.index(row["municipio"]) + 1}, "{row["municipio"]}", {row["estado"]});', file=writer)
            row["municipio"] = lista_municipios.index(row["municipio"]) + 1
            
            if not row["categoria_giro"] in lista_categoria:
                lista_categoria.append(row['categoria_giro'])
                print(f'insert into categoria(id_categoria, nombre) values ({lista_categoria.index(row["categoria_giro"]) + 1}, "{row["categoria_giro"]}");', file=writer)
            row["categoria_giro"] = lista_categoria.index(row["categoria_giro"]) + 1

            if not row["giro"] in lista_giro:
                lista_giro.append(row['giro'])
                print(f'insert into giro(id_giro, nombre, id_categoria) values ({lista_giro.index(row["giro"]) + 1}, "{row["giro"]}", {row["categoria_giro"]});', file=writer)
            row["giro"] = lista_giro.index(row["giro"]) + 1

            contacto_rep = (row["email_representante"], row["tel_representante"])
            contacto_neg = (row["email_negocio"], row["telefono_negocio"])
            representante = (row["representante_legal"], row["tel_representante"])
            
            if not contacto_rep in lista_contacto:
                lista_contacto.append(contacto_rep)
                print(f'insert into contacto(id_contacto, email, telefono) values ({lista_contacto.index(contacto_rep) + 1}, "{row["email_representante"]}", "{row["tel_representante"]}");', file=writer)
            id_contacto_rep = lista_contacto.index(contacto_rep) + 1

            if not contacto_neg in lista_contacto:
                lista_contacto.append(contacto_neg)
                print(f'insert into contacto(id_contacto, email, telefono) values ({lista_contacto.index(contacto_neg)+1},"{row["email_negocio"]}", "{row["telefono_negocio"]}");', file=writer)
            id_contacto_neg = lista_contacto.index(contacto_neg) + 1

            if not representante in lista_representante:
                lista_representante.append(representante)
                print(f'insert into representante(id_representante, nombre, id_contacto) values ({lista_representante.index(representante) + 1}, "{row["representante_legal"]}", {id_contacto_rep});', file=writer)
            row["representante"] = lista_representante.index(representante) + 1
            
            fecha = row["fecha_alta"].split("/")
            fecha_sql = f"{fecha[2]}-{fecha[1]}-{fecha[0]}"

            print(
            f'''INSERT INTO Negocio(
            Nombre,RFC,Razon_social,Direccion,Num_Empleados,Capital_inicial,
            Fecha_alta,Status,
            ID_Contacto,ID_Representante,ID_Giro,ID_Municipio)
            VALUES
            ("{row["nombre_negocio"]}","{row["rfc"]}","{row["razon_social"]}","{row["direccion"]}",
            {row["num_empleados"]},{row["capital_inicial"]},"{fecha_sql}",
            "{row["estatus"]}",
            {id_contacto_neg},{row["representante"]},{row["giro"]},{row["municipio"]});''',
            file=writer
            )
            total_negocios += 1
       
        print("total estados:", len(lista_estados))
        print("total municipios:", len(lista_municipios))
        print("total zonas:", len(lista_zona))
        print("total regiones:", len(lista_region))
        print("total categoria:", len(lista_categoria))
        print("total giros:", len(lista_giro))
        print("total representantes:", len(lista_representante))
        print("total contactos:", len(lista_contacto))
        print("total negocios:", total_negocios)

  #print(f"insert into negocio(id_negocio,nombre_negocio,rfc,razon_social) Values (null,{row['nombre_negocio']},{row['rfc']},{row['razon_social']}');", file=writer)
