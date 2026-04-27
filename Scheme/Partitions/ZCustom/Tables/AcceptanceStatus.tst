<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="6ef0270f-dbdf-40bb-be8c-74b21c376fdf" Name="AcceptanceStatus" Group="Custom" InstanceType="Cards" ContentType="Entries">
	<Description>Статус принятия</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="6ef0270f-dbdf-00bb-2000-04b21c376fdf" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="6ef0270f-dbdf-01bb-4000-04b21c376fdf" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="c61a2a8b-495d-4512-807e-9a7fa3dd2bb8" Name="Name" Type="String(Max) Null" />
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="6ef0270f-dbdf-00bb-5000-04b21c376fdf" Name="pk_AcceptanceStatus" IsClustered="true">
		<SchemeIndexedColumn Column="6ef0270f-dbdf-01bb-4000-04b21c376fdf" />
	</SchemePrimaryKey>
</SchemeTable>