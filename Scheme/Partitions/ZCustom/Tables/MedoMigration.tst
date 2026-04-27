<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="ac12c882-2ba7-434b-9a97-eb9985a8e093" Name="MedoMigration" Group="Custom" InstanceType="Cards" ContentType="Collections">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="ac12c882-2ba7-004b-2000-0b9985a8e093" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="ac12c882-2ba7-014b-4000-0b9985a8e093" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="ac12c882-2ba7-004b-3100-0b9985a8e093" Name="RowID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="1fbee392-3b77-4891-b97a-0c0966015ab8" Name="RowIDToMigrate" Type="Guid Null" />
	<SchemePhysicalColumn ID="33d74798-ef45-4bf0-8672-962b033bc56a" Name="RowTable" Type="Int32 Null" />
	<SchemePhysicalColumn ID="f02e787c-12f2-4ca8-843c-2f37baf79296" Name="RowAction" Type="Int32 Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="4da6ee20-6caa-4304-a009-8980adc5f1e6" Name="StatusID" Type="Int32 Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="4c8d63b6-a5e1-48ef-8a31-2327dd82841a" Name="StatusName" Type="String(512) Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="67c9d913-d881-49b5-8bcf-5bc3e74237ef" Name="SendDateTime" Type="DateTime Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="af0017f8-b389-4b85-b9d9-3106997e7302" Name="PassportName" Type="String(Max) Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="c37384e8-1f66-47f9-a0ed-fdd3cacc7171" Name="PassportContent" Type="String(Max) Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="a06766a5-5511-47e9-badc-d3fdb3765f6a" Name="MigrationTypeID" Type="Int32 Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="586beff6-429d-4188-ba1a-7dc89dbeaa5d" Name="MigrationTypeName" Type="String(512) Null" />
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="ac12c882-2ba7-004b-5000-0b9985a8e093" Name="pk_MedoMigration">
		<SchemeIndexedColumn Column="ac12c882-2ba7-004b-3100-0b9985a8e093" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="ac12c882-2ba7-004b-7000-0b9985a8e093" Name="idx_MedoMigration_ID" IsClustered="true">
		<SchemeIndexedColumn Column="ac12c882-2ba7-014b-4000-0b9985a8e093" />
	</SchemeIndex>
</SchemeTable>