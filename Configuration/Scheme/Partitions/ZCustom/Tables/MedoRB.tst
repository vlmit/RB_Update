<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="8e09b556-dff9-41e9-adbd-3c9bcafcdf74" Name="MedoRB" Group="Common" InstanceType="Cards" ContentType="Entries">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="8e09b556-dff9-00e9-2000-0c9bcafcdf74" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="8e09b556-dff9-01e9-4000-0c9bcafcdf74" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="35652c4f-f339-4df2-8178-47b4dee81a2f" Name="Partner" Type="Reference(Typified) Null" ReferencedTable="5d47ef13-b6f4-47ef-9815-3b3d0e6d475a">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="35652c4f-f339-00f2-4000-07b4dee81a2f" Name="PartnerID" Type="Guid Null" ReferencedColumn="5d47ef13-b6f4-01ef-4000-0b3d0e6d475a" />
		<SchemeReferencingColumn ID="0b09f0d6-89dd-4bbd-bb41-b1c7874cceb0" Name="PartnerName" Type="String(255) Null" ReferencedColumn="f1c960e0-951e-4837-8474-bb61d98f40f0" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="2a5bfd05-4188-4c9f-a1ea-b5111b5bb6f7" Name="CopyFolder" Type="String(Max) Null" />
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="8e09b556-dff9-00e9-5000-0c9bcafcdf74" Name="pk_MedoRB" IsClustered="true">
		<SchemeIndexedColumn Column="8e09b556-dff9-01e9-4000-0c9bcafcdf74" />
	</SchemePrimaryKey>
</SchemeTable>