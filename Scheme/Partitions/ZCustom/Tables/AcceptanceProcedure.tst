<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="66508867-2104-4c64-b739-12f969f6dfe8" Name="AcceptanceProcedure" Group="Custom" InstanceType="Cards" ContentType="Entries">
	<Description>Порядок принятия</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="66508867-2104-0064-2000-02f969f6dfe8" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="66508867-2104-0164-4000-02f969f6dfe8" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="ed6d020b-023d-43af-abf4-c2ef98acf362" Name="Name" Type="String(Max) Null" />
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="66508867-2104-0064-5000-02f969f6dfe8" Name="pk_AcceptanceProcedure" IsClustered="true">
		<SchemeIndexedColumn Column="66508867-2104-0164-4000-02f969f6dfe8" />
	</SchemePrimaryKey>
</SchemeTable>