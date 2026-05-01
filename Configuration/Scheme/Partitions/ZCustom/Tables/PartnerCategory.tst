<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="e8d856c2-3f2d-40d9-97f7-f5ee08b65301" Name="PartnerCategory" Group="Custom" InstanceType="Cards" ContentType="Entries">
	<Description>Категории для контрагентов</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="e8d856c2-3f2d-00d9-2000-05ee08b65301" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="e8d856c2-3f2d-01d9-4000-05ee08b65301" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="a3668609-e274-40d5-bf62-3c1cc57ad793" Name="Name" Type="String(128) Not Null" />
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="e8d856c2-3f2d-00d9-5000-05ee08b65301" Name="pk_PartnerCategory" IsClustered="true">
		<SchemeIndexedColumn Column="e8d856c2-3f2d-01d9-4000-05ee08b65301" />
	</SchemePrimaryKey>
</SchemeTable>