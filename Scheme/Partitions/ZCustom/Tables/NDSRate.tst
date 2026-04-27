<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="b7404c8f-eef1-48ec-a750-cbcbeb8be283" Name="NDSRate" Group="Custom" InstanceType="Cards" ContentType="Entries">
	<Description>Ставка ДНС</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="b7404c8f-eef1-00ec-2000-0bcbeb8be283" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="b7404c8f-eef1-01ec-4000-0bcbeb8be283" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="ea691237-6500-4f85-b357-af3018781279" Name="Rate" Type="String(Max) Null">
		<Description>Ставка</Description>
	</SchemePhysicalColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="b7404c8f-eef1-00ec-5000-0bcbeb8be283" Name="pk_NDSRate" IsClustered="true">
		<SchemeIndexedColumn Column="b7404c8f-eef1-01ec-4000-0bcbeb8be283" />
	</SchemePrimaryKey>
</SchemeTable>