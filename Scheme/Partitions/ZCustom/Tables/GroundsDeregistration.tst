<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="b7e096ef-ff8d-4f9c-9c19-d27c2dc9758e" Name="GroundsDeregistration" Group="Custom">
	<Description>Основание для снятия с контроля</Description>
	<SchemePhysicalColumn ID="52039ceb-78ce-4ee5-9ef0-1d1ea3bd866f" Name="ID" Type="Int16 Not Null" />
	<SchemePhysicalColumn ID="4a7ca97b-2f68-4e2d-937a-af09ec77c1fa" Name="Name" Type="String(128) Not Null">
		<Description>1</Description>
	</SchemePhysicalColumn>
	<SchemePrimaryKey ID="3ea068ed-e9b3-4203-80a0-4b6f8ac6e946" Name="pk_GroundsDeregistration">
		<SchemeIndexedColumn Column="52039ceb-78ce-4ee5-9ef0-1d1ea3bd866f" />
	</SchemePrimaryKey>
	<SchemeRecord>
		<ID ID="52039ceb-78ce-4ee5-9ef0-1d1ea3bd866f">0</ID>
		<Name ID="4a7ca97b-2f68-4e2d-937a-af09ec77c1fa">Исполнено по существу</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="52039ceb-78ce-4ee5-9ef0-1d1ea3bd866f">1</ID>
		<Name ID="4a7ca97b-2f68-4e2d-937a-af09ec77c1fa">Передано на вн.вед. контроль</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="52039ceb-78ce-4ee5-9ef0-1d1ea3bd866f">2</ID>
		<Name ID="4a7ca97b-2f68-4e2d-937a-af09ec77c1fa">Исполнение невозможно/нецелесообразно</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="52039ceb-78ce-4ee5-9ef0-1d1ea3bd866f">3</ID>
		<Name ID="4a7ca97b-2f68-4e2d-937a-af09ec77c1fa">Контроль продолжен в др. документах</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="52039ceb-78ce-4ee5-9ef0-1d1ea3bd866f">4</ID>
		<Name ID="4a7ca97b-2f68-4e2d-937a-af09ec77c1fa">Периодическое направление информации</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="52039ceb-78ce-4ee5-9ef0-1d1ea3bd866f">5</ID>
		<Name ID="4a7ca97b-2f68-4e2d-937a-af09ec77c1fa">По нескольким основаниям</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="52039ceb-78ce-4ee5-9ef0-1d1ea3bd866f">6</ID>
		<Name ID="4a7ca97b-2f68-4e2d-937a-af09ec77c1fa">Другие основания</Name>
	</SchemeRecord>
</SchemeTable>