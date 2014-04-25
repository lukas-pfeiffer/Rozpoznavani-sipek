Rozpoznávání šipek
==================

Cílem projektu bylo vytvořit aplikaci, která rozpoznává objekty v obraze (šipky - levá, pravá, trojuhelník), které jsou ve formátu RAW a pochází z robotů Koala.

V aplikace bylo nutné nejprve načíst a zpracovat data se souboru RAW a poté zvolit správnou segmentaci obrazu pomocí vhodné konvoluce a konvoluční masky. Poté již podle tvaru přesně určit zda se jedná o šipku doprava, doleva či trojuhelník.
