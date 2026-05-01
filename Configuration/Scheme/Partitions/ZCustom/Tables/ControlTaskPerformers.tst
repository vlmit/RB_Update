<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="d68ba939-0ac9-4612-b436-413b3cb00d09" Name="ControlTaskPerformers" Group="Custom" InstanceType="Cards" ContentType="Collections">
	<Description>Исполнители</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="d68ba939-0ac9-0012-2000-013b3cb00d09" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="d68ba939-0ac9-0112-4000-013b3cb00d09" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="d68ba939-0ac9-0012-3100-013b3cb00d09" Name="RowID" Type="Guid Not Null" />
	<SchemeComplexColumn ID="fc9d4748-e6b2-4261-9028-12cadf2c2ed4" Name="User" Type="Reference(Typified) Not Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="fc9d4748-e6b2-0061-4000-02cadf2c2ed4" Name="UserID" Type="Guid Not Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="afc34155-262c-4b51-a285-670436b2a6d4" Name="UserName" Type="String(128) Not Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964" />
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="00de63ad-e012-4ba7-8fe8-166b162592b4" Name="Parent" Type="Reference(Typified) Not Null" ReferencedTable="e38fa5d8-fdfd-4123-a555-c4cb9f5d69cc" IsReferenceToOwner="true">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="00de63ad-e012-00a7-4000-066b162592b4" Name="ParentRowID" Type="Guid Not Null" ReferencedColumn="e38fa5d8-fdfd-0023-3100-04cb9f5d69cc" />
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="d68ba939-0ac9-0012-5000-013b3cb00d09" Name="pk_ControlTaskPerformers">
		<SchemeIndexedColumn Column="d68ba939-0ac9-0012-3100-013b3cb00d09" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="d68ba939-0ac9-0012-7000-013b3cb00d09" Name="idx_ControlTaskPerformers_ID" IsClustered="true">
		<SchemeIndexedColumn Column="d68ba939-0ac9-0112-4000-013b3cb00d09" />
	</SchemeIndex>
</SchemeTable>