<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="11cd2fc8-50f2-42b3-8df0-85a3139ba407" Name="SignFilesMobile" Group="Custom" InstanceType="Cards" ContentType="Collections">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="11cd2fc8-50f2-00b3-2000-05a3139ba407" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="11cd2fc8-50f2-01b3-4000-05a3139ba407" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="11cd2fc8-50f2-00b3-3100-05a3139ba407" Name="RowID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="c8d3a448-d7bc-42b2-b1a3-b2a17aa8a77a" Name="SignedFileName" Type="String(Max) Null" />
	<SchemePhysicalColumn ID="f6cb9aa9-3aa9-48a0-a7b6-ed5d542503a9" Name="SignContent" Type="String(Max) Null" />
	<SchemePhysicalColumn ID="b44cf21f-fdc1-428e-8ea6-48c9b807325a" Name="SignType" Type="String(Max) Null" />
	<SchemePhysicalColumn ID="74d6953a-50d1-4442-9464-e86e491093b6" Name="SignDate" Type="String(Max) Null" />
	<SchemePhysicalColumn ID="2df08dec-d179-4364-a39d-eeca9a7848f7" Name="SignedBy" Type="String(Max) Null" />
	<SchemePhysicalColumn ID="47e351f0-aa05-4b01-a0c2-337a4c5e1f38" Name="IsProcessed" Type="Boolean Null" />
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="11cd2fc8-50f2-00b3-5000-05a3139ba407" Name="pk_SignFilesMobile">
		<SchemeIndexedColumn Column="11cd2fc8-50f2-00b3-3100-05a3139ba407" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="11cd2fc8-50f2-00b3-7000-05a3139ba407" Name="idx_SignFilesMobile_ID" IsClustered="true">
		<SchemeIndexedColumn Column="11cd2fc8-50f2-01b3-4000-05a3139ba407" />
	</SchemeIndex>
</SchemeTable>