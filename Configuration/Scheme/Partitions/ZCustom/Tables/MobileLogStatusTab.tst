<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="c7af5a3f-1c43-4f6c-95c6-512205df9cda" Name="MobileLogStatusTab" Group="Custom" InstanceType="Cards" ContentType="Collections">
	<Description>Запись флагов для обновления РМ Мобильный кабинет</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="c7af5a3f-1c43-006c-2000-012205df9cda" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="c7af5a3f-1c43-016c-4000-012205df9cda" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="c7af5a3f-1c43-006c-3100-012205df9cda" Name="RowID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="2f0f778b-262f-4b21-807d-f737a37dac26" Name="CurrentUserID" Type="Guid Null" />
	<SchemePhysicalColumn ID="531319e2-1cf2-4ffe-af9c-83083c4efc10" Name="StatusMob" Type="Boolean Null" />
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="c7af5a3f-1c43-006c-5000-012205df9cda" Name="pk_MobileLogStatusTab">
		<SchemeIndexedColumn Column="c7af5a3f-1c43-006c-3100-012205df9cda" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="c7af5a3f-1c43-006c-7000-012205df9cda" Name="idx_MobileLogStatusTab_ID" IsClustered="true">
		<SchemeIndexedColumn Column="c7af5a3f-1c43-016c-4000-012205df9cda" />
	</SchemeIndex>
</SchemeTable>