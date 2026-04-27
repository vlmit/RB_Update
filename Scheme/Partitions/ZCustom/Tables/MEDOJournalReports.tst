<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="b26be1f7-f51e-4d0f-8ad4-541649216a64" Name="MEDOJournalReports" Group="Custom" InstanceType="Cards">
	<Description>Журнал докладов МЭДО</Description>
	<SchemeComplexColumn ID="ec181183-a0f8-4344-9ce9-595246156cb4" Name="Partners" Type="Reference(Typified) Null" ReferencedTable="5d47ef13-b6f4-47ef-9815-3b3d0e6d475a">
		<Description>Адресат</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="ec181183-a0f8-0044-4000-095246156cb4" Name="PartnersID" Type="Guid Null" ReferencedColumn="5d47ef13-b6f4-01ef-4000-0b3d0e6d475a" />
		<SchemeReferencingColumn ID="98216161-63fc-4a11-b135-bfe28e2d360e" Name="PartnersName" Type="String(255) Null" ReferencedColumn="f1c960e0-951e-4837-8474-bb61d98f40f0" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="aeb12d60-eb60-4eb1-ad61-5f67dafb0023" Name="DateOfReceiving" Type="DateTime Null">
		<Description>Дата получения </Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="94b2f2fe-97ac-41f6-9766-29ed5512fc1a" Name="Comment" Type="String(Max) Null">
		<Description>Комментарий </Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="b8cfce52-89ca-4300-a64d-6abe76a669db" Name="Status" Type="String(Max) Null">
		<Description>Статус </Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="4bc42f48-4fbc-4e75-ae4b-5a9baa918ed4" Name="RegNum" Type="String(Max) Null">
		<Description>Рег.№ </Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="5debc3fd-f2fd-468b-9259-51451289cd75" Name="ID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="331e2c8b-b322-4b45-bc56-e47443dab93e" Name="CardID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="4953fac6-26f6-4ed1-8424-4aefea045aaf" Name="MesID" Type="Guid Null" />
	<SchemePhysicalColumn ID="11b19471-8c39-4c05-b712-c7b42f3e0062" Name="SedRowID" Type="Guid Null" />
	<SchemePrimaryKey ID="af2636e5-c6ed-4e4f-ab54-e47b0bda08cb" Name="pk_MEDOJournalReports">
		<SchemeIndexedColumn Column="5debc3fd-f2fd-468b-9259-51451289cd75" />
	</SchemePrimaryKey>
</SchemeTable>