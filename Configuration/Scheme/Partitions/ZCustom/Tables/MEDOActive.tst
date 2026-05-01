<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="4d654422-f61b-4d84-8de7-527ec119a244" Name="MEDOActive" Group="Custom">
	<Description>Статус МЭДО</Description>
	<SchemePhysicalColumn ID="e08582a4-d97d-4c82-91cf-0ad9e38937e8" Name="ID" Type="Int16 Null" />
	<SchemePhysicalColumn ID="5680537b-d8db-43f9-9b77-e1a62f7f8f23" Name="ActiveStatus" Type="String(Max) Null" />
	<SchemePrimaryKey ID="b7cfd662-c963-4422-9fe2-7141f312c771" Name="pk_MEDOActive">
		<SchemeIndexedColumn Column="e08582a4-d97d-4c82-91cf-0ad9e38937e8" />
	</SchemePrimaryKey>
	<SchemeRecord>
		<ID ID="e08582a4-d97d-4c82-91cf-0ad9e38937e8">0</ID>
		<ActiveStatus ID="5680537b-d8db-43f9-9b77-e1a62f7f8f23">Активен</ActiveStatus>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="e08582a4-d97d-4c82-91cf-0ad9e38937e8">1</ID>
		<ActiveStatus ID="5680537b-d8db-43f9-9b77-e1a62f7f8f23">Неактивен</ActiveStatus>
	</SchemeRecord>
</SchemeTable>