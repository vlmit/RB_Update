<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="44f7bcb1-4d9b-4b23-8b68-5280716dc9c1" Name="SubState" Group="Custom">
	<Description>Подсостояния</Description>
	<SchemePhysicalColumn ID="90bb6920-db81-40e6-8e2c-4838a03337be" Name="ID" Type="Int16 Not Null" />
	<SchemePhysicalColumn ID="e7523bce-a82b-49ee-aa7b-c043106663ea" Name="Name" Type="String(128) Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="e2378c41-4b35-4cce-8849-a60a0624bcd4" Name="df_SubState_Name" Value="Не запущен" />
	</SchemePhysicalColumn>
	<SchemePrimaryKey ID="bfb23fe0-8398-4e78-a602-5e338748c3ce" Name="pk_SubState">
		<SchemeIndexedColumn Column="90bb6920-db81-40e6-8e2c-4838a03337be" />
	</SchemePrimaryKey>
	<SchemeRecord>
		<ID ID="90bb6920-db81-40e6-8e2c-4838a03337be">0</ID>
		<Name ID="e7523bce-a82b-49ee-aa7b-c043106663ea">Не начато</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="90bb6920-db81-40e6-8e2c-4838a03337be">1</ID>
		<Name ID="e7523bce-a82b-49ee-aa7b-c043106663ea">На исполнении</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="90bb6920-db81-40e6-8e2c-4838a03337be">2</ID>
		<Name ID="e7523bce-a82b-49ee-aa7b-c043106663ea">Исполнено</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="90bb6920-db81-40e6-8e2c-4838a03337be">3</ID>
		<Name ID="e7523bce-a82b-49ee-aa7b-c043106663ea">Переназначено</Name>
	</SchemeRecord>
</SchemeTable>