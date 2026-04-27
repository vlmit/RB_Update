<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="0f4ed2df-09d1-4fea-ac3a-acb386f0ab6b" Name="ContractType" Group="Custom">
	<Description>Тип договора</Description>
	<SchemePhysicalColumn ID="3c0100c8-4e44-4d9b-9c05-a82edf5a25c7" Name="ID" Type="Int16 Not Null" />
	<SchemePhysicalColumn ID="6b8b2eca-929b-43de-a91e-bf857422c64e" Name="Name" Type="String(Max) Not Null" />
	<SchemePrimaryKey ID="39e71e75-a7d3-4632-9c58-a2e32fbb3295" Name="pk_ContractType">
		<SchemeIndexedColumn Column="3c0100c8-4e44-4d9b-9c05-a82edf5a25c7" />
	</SchemePrimaryKey>
	<SchemeRecord>
		<ID ID="3c0100c8-4e44-4d9b-9c05-a82edf5a25c7">1</ID>
		<Name ID="6b8b2eca-929b-43de-a91e-bf857422c64e">Купля / продажа</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="3c0100c8-4e44-4d9b-9c05-a82edf5a25c7">2</ID>
		<Name ID="6b8b2eca-929b-43de-a91e-bf857422c64e">Поставка</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="3c0100c8-4e44-4d9b-9c05-a82edf5a25c7">3</ID>
		<Name ID="6b8b2eca-929b-43de-a91e-bf857422c64e">Оказание услуг</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="3c0100c8-4e44-4d9b-9c05-a82edf5a25c7">4</ID>
		<Name ID="6b8b2eca-929b-43de-a91e-bf857422c64e">Аренда</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="3c0100c8-4e44-4d9b-9c05-a82edf5a25c7">5</ID>
		<Name ID="6b8b2eca-929b-43de-a91e-bf857422c64e">Лизинг</Name>
	</SchemeRecord>
</SchemeTable>