<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="820ce1b5-30b5-473a-8a7e-d8a3b2c77201" Name="RescheduleLog" Group="Custom" InstanceType="Cards" ContentType="Collections">
	<Description>Журнал переноса сроков</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="820ce1b5-30b5-003a-2000-08a3b2c77201" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="820ce1b5-30b5-013a-4000-08a3b2c77201" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="820ce1b5-30b5-003a-3100-08a3b2c77201" Name="RowID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="84cffec0-33b6-476d-9d52-1cfa40deed60" Name="Order" Type="Int32 Not Null">
		<Description>Порядок решений в списке</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="6cb120d4-c7d4-4157-8420-d2caf7cf5451" Name="df_RescheduleLog_Order" Value="0" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="0aa2fa8f-080a-4b73-a208-ab8414fc5809" Name="InitialDeadline" Type="Date Null">
		<Description>Первоначальный срок исполнения поручения</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="9a5435bb-b914-4f47-80d4-42b2da20501e" Name="NewDeadline" Type="Date Null">
		<Description>Новый срок исполнения поручения</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="138d8b75-2dd6-451c-9658-422a1bd1afc3" Name="Comment" Type="String(2048) Null">
		<Description>Комментарий </Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="eb45f4aa-7ebd-49c8-b4b4-e4339c093258" Name="Doc" Type="Reference(Typified) Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e" WithForeignKey="false">
		<Description>Электронный документ – основание переноса скока</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="eb45f4aa-7ebd-00c8-4000-04339c093258" Name="DocID" Type="Guid Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
		<SchemePhysicalColumn ID="ad63db8c-5ed9-461f-9b2f-55eebad04134" Name="DocDescription" Type="String(1024) Null" />
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="820ce1b5-30b5-003a-5000-08a3b2c77201" Name="pk_RescheduleLog">
		<SchemeIndexedColumn Column="820ce1b5-30b5-003a-3100-08a3b2c77201" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="820ce1b5-30b5-003a-7000-08a3b2c77201" Name="idx_RescheduleLog_ID" IsClustered="true">
		<SchemeIndexedColumn Column="820ce1b5-30b5-013a-4000-08a3b2c77201" />
	</SchemeIndex>
</SchemeTable>