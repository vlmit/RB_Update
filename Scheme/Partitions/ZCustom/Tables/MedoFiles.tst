<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="0a001db7-9071-4436-ba1a-23cb26ccd33d" Name="MedoFiles" Group="Custom" InstanceType="Cards" ContentType="Collections">
	<Description>Приложения для организаций</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="0a001db7-9071-0036-2000-03cb26ccd33d" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="0a001db7-9071-0136-4000-03cb26ccd33d" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="0a001db7-9071-0036-3100-03cb26ccd33d" Name="RowID" Type="Guid Not Null" />
	<SchemeComplexColumn ID="7efde476-9ca9-4986-90f7-828a41f54348" Name="Parent" Type="Reference(Typified) Null" ReferencedTable="a74ad1f9-b22e-4b28-a914-2a1e27101101" IsReferenceToOwner="true">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="7efde476-9ca9-0086-4000-028a41f54348" Name="ParentRowID" Type="Guid Null" ReferencedColumn="a74ad1f9-b22e-0028-3100-0a1e27101101" />
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="824dbdbd-3150-4b4f-aa23-caa66de7de63" Name="Files" Type="Reference(Typified) Null" ReferencedTable="dd716146-b177-4920-bc90-b1196b16347c">
		<Description>Файлы</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="824dbdbd-3150-004f-4000-0aa66de7de63" Name="FilesID" Type="Guid Null" ReferencedColumn="dd716146-b177-0020-3100-01196b16347c" />
		<SchemeReferencingColumn ID="dee1d0cd-f2c7-4824-818f-cb71e1b1a8e2" Name="FilesName" Type="String(256) Null" ReferencedColumn="5fa1d976-21b8-4df5-b52e-f7beadf93e9d" />
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="0a001db7-9071-0036-5000-03cb26ccd33d" Name="pk_MedoFiles">
		<SchemeIndexedColumn Column="0a001db7-9071-0036-3100-03cb26ccd33d" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="0a001db7-9071-0036-7000-03cb26ccd33d" Name="idx_MedoFiles_ID" IsClustered="true">
		<SchemeIndexedColumn Column="0a001db7-9071-0136-4000-03cb26ccd33d" />
	</SchemeIndex>
</SchemeTable>