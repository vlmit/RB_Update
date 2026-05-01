<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="d9c38557-c1c1-441b-8699-e4e3e0619ee3" Name="Urgency" Group="Custom" InstanceType="Cards" ContentType="Entries">
	<Description>Срочност</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="d9c38557-c1c1-001b-2000-04e3e0619ee3" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="d9c38557-c1c1-011b-4000-04e3e0619ee3" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="22caea1f-324a-43aa-99b3-e7f8c54c1994" Name="Name" Type="String(Max) Null">
		<Description>12345</Description>
	</SchemePhysicalColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="d9c38557-c1c1-001b-5000-04e3e0619ee3" Name="pk_Urgency" IsClustered="true">
		<SchemeIndexedColumn Column="d9c38557-c1c1-011b-4000-04e3e0619ee3" />
	</SchemePrimaryKey>
</SchemeTable>