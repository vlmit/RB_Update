<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="f88137b6-5605-4e46-9408-5d74e0c312ef" Name="ProjectQuality" Group="Custom" InstanceType="Cards" ContentType="Entries">
	<Description>Качество подготовик проекта </Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="f88137b6-5605-0046-2000-0d74e0c312ef" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="f88137b6-5605-0146-4000-0d74e0c312ef" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="ca4950e0-4f74-4f94-aa05-ba13b6644738" Name="Name" Type="String(Max) Null" />
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="f88137b6-5605-0046-5000-0d74e0c312ef" Name="pk_ProjectQuality" IsClustered="true">
		<SchemeIndexedColumn Column="f88137b6-5605-0146-4000-0d74e0c312ef" />
	</SchemePrimaryKey>
</SchemeTable>