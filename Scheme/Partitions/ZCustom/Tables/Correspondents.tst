<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="56e5d379-dc1c-4d0b-bfca-657cccd0df2c" Name="Correspondents" Group="Custom" InstanceType="Cards" ContentType="Collections">
	<Description>Корреспонденты</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="56e5d379-dc1c-000b-2000-057cccd0df2c" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="56e5d379-dc1c-010b-4000-057cccd0df2c" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="56e5d379-dc1c-000b-3100-057cccd0df2c" Name="RowID" Type="Guid Not Null" />
	<SchemeComplexColumn ID="5e5cedec-e62d-4bd3-833e-10ce9c73e9ad" Name="Partners" Type="Reference(Typified) Not Null" ReferencedTable="5d47ef13-b6f4-47ef-9815-3b3d0e6d475a">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="5e5cedec-e62d-00d3-4000-00ce9c73e9ad" Name="PartnersID" Type="Guid Not Null" ReferencedColumn="5d47ef13-b6f4-01ef-4000-0b3d0e6d475a" />
		<SchemeReferencingColumn ID="353299ef-1a09-4b7c-9677-e5b952be17c6" Name="PartnersName" Type="String(255) Not Null" ReferencedColumn="f1c960e0-951e-4837-8474-bb61d98f40f0" />
		<SchemeReferencingColumn ID="bc08f189-ef8b-4ab3-9bca-b4077e663ce7" Name="PartnersMedoID" Type="String(Max) Null" ReferencedColumn="745788ea-1a23-472c-9eb0-c48d2dfea546" />
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="56e5d379-dc1c-000b-5000-057cccd0df2c" Name="pk_Correspondents">
		<SchemeIndexedColumn Column="56e5d379-dc1c-000b-3100-057cccd0df2c" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="56e5d379-dc1c-000b-7000-057cccd0df2c" Name="idx_Correspondents_ID" IsClustered="true">
		<SchemeIndexedColumn Column="56e5d379-dc1c-010b-4000-057cccd0df2c" />
	</SchemeIndex>
</SchemeTable>