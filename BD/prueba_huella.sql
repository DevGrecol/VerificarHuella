PGDMP         #                }            prueba_huella    13.8    13.8     �           0    0    ENCODING    ENCODING        SET client_encoding = 'UTF8';
                      false            �           0    0 
   STDSTRINGS 
   STDSTRINGS     (   SET standard_conforming_strings = 'on';
                      false            �           0    0 
   SEARCHPATH 
   SEARCHPATH     8   SELECT pg_catalog.set_config('search_path', '', false);
                      false            �           1262    91750    prueba_huella    DATABASE     l   CREATE DATABASE prueba_huella WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE = 'Spanish_Colombia.1252';
    DROP DATABASE prueba_huella;
                postgres    false            �            1259    91751    huella    TABLE     o   CREATE TABLE public.huella (
    "Id" integer NOT NULL,
    nombre text NOT NULL,
    huella bytea NOT NULL
);
    DROP TABLE public.huella;
       public         heap    postgres    false            �          0    91751    huella 
   TABLE DATA           6   COPY public.huella ("Id", nombre, huella) FROM stdin;
    public          postgres    false    200   3       "           2606    91758    huella huella_pkey 
   CONSTRAINT     R   ALTER TABLE ONLY public.huella
    ADD CONSTRAINT huella_pkey PRIMARY KEY ("Id");
 <   ALTER TABLE ONLY public.huella DROP CONSTRAINT huella_pkey;
       public            postgres    false    200            �      x������ � �     