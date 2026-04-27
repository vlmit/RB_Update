<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="bba5210d-6c55-4ff0-9377-360827273371" Name="MEDODSP" Group="Custom">
	<Description>Работа с НПА</Description>
	<SchemePhysicalColumn ID="7cab56e9-0c6b-47f9-8b33-3b1432d7ccdc" Name="ID" Type="Int16 Null" />
	<SchemePhysicalColumn ID="df03da73-75bc-4115-8b81-38b897a0392e" Name="DSP" Type="String(Max) Null" />
	<SchemePrimaryKey ID="81dbc19e-d6f1-411c-acaf-6ffff54e1f11" Name="pk_MEDODSP">
		<SchemeIndexedColumn Column="7cab56e9-0c6b-47f9-8b33-3b1432d7ccdc" />
	</SchemePrimaryKey>
	<SchemeRecord>
		<ID ID="7cab56e9-0c6b-47f9-8b33-3b1432d7ccdc">0</ID>
		<DSP ID="df03da73-75bc-4115-8b81-38b897a0392e">ДСП</DSP>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="7cab56e9-0c6b-47f9-8b33-3b1432d7ccdc">1</ID>
		<DSP ID="df03da73-75bc-4115-8b81-38b897a0392e">Обычный доступ</DSP>
	</SchemeRecord>
</SchemeTable>