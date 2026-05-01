<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="d39124d1-314a-4414-a910-c31979c15c35" Name="Approvers" Group="Common" InstanceType="Cards" ContentType="Collections">
	<Description>Согласующие</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="d39124d1-314a-0014-2000-031979c15c35" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="d39124d1-314a-0114-4000-031979c15c35" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="d39124d1-314a-0014-3100-031979c15c35" Name="RowID" Type="Guid Not Null" />
	<SchemeComplexColumn ID="5360cc89-0891-48ef-8585-6ae590483640" Name="User" Type="Reference(Typified) Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="5360cc89-0891-00ef-4000-0ae590483640" Name="UserID" Type="Guid Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="7a3f81ec-2f73-432e-af01-06628fd932fa" Name="UserName" Type="String(128) Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964" />
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="d39124d1-314a-0014-5000-031979c15c35" Name="pk_Approvers">
		<SchemeIndexedColumn Column="d39124d1-314a-0014-3100-031979c15c35" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="d39124d1-314a-0014-7000-031979c15c35" Name="idx_Approvers_ID" IsClustered="true">
		<SchemeIndexedColumn Column="d39124d1-314a-0114-4000-031979c15c35" />
	</SchemeIndex>
</SchemeTable>