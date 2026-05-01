<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="533ab008-ae97-4d2c-9148-fe3c11f98a05" Name="MEDOFormat" Group="Custom">
	<Description>Формат МЭДО</Description>
	<SchemePhysicalColumn ID="398ecbff-7e39-4e67-a7ad-d1ac1e0c4215" Name="ID" Type="Int16 Null" />
	<SchemePhysicalColumn ID="3203c29c-b404-485c-82d3-542c1532b572" Name="Format" Type="String(Max) Not Null" />
	<SchemePrimaryKey ID="ac7f3e45-9f65-485f-9e84-c309f4c713a3" Name="pk_MEDOFormat">
		<SchemeIndexedColumn Column="398ecbff-7e39-4e67-a7ad-d1ac1e0c4215" />
	</SchemePrimaryKey>
	<SchemeRecord>
		<ID ID="398ecbff-7e39-4e67-a7ad-d1ac1e0c4215">0</ID>
		<Format ID="3203c29c-b404-485c-82d3-542c1532b572">2.2</Format>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="398ecbff-7e39-4e67-a7ad-d1ac1e0c4215">1</ID>
		<Format ID="3203c29c-b404-485c-82d3-542c1532b572">2.7</Format>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="398ecbff-7e39-4e67-a7ad-d1ac1e0c4215">2</ID>
		<Format ID="3203c29c-b404-485c-82d3-542c1532b572">2.7.1</Format>
	</SchemeRecord>
</SchemeTable>