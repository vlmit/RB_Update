<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="c4c0ca80-0b2e-43c1-b09d-28d57ff13ed1" Name="TaskAttachmentFiles" Group="Custom" InstanceType="Tasks" ContentType="Collections">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="c4c0ca80-0b2e-00c1-2000-08d57ff13ed1" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="5bfa9936-bb5a-4e8f-89a9-180bfd8f75f8">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="c4c0ca80-0b2e-01c1-4000-08d57ff13ed1" Name="ID" Type="Guid Not Null" ReferencedColumn="5bfa9936-bb5a-008f-3100-080bfd8f75f8" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="c4c0ca80-0b2e-00c1-3100-08d57ff13ed1" Name="RowID" Type="Guid Not Null" />
	<SchemeComplexColumn ID="0cb66fc1-7f5b-4bba-9589-8b668f449cd9" Name="Files" Type="Reference(Typified) Null" ReferencedTable="dd716146-b177-4920-bc90-b1196b16347c">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="0cb66fc1-7f5b-00ba-4000-0b668f449cd9" Name="FilesID" Type="Guid Null" ReferencedColumn="dd716146-b177-0020-3100-01196b16347c" />
		<SchemeReferencingColumn ID="d62f9e64-97b4-44be-8a01-c96b776ce5ca" Name="FilesName" Type="String(256) Null" ReferencedColumn="5fa1d976-21b8-4df5-b52e-f7beadf93e9d" />
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="c4c0ca80-0b2e-00c1-5000-08d57ff13ed1" Name="pk_TaskAttachmentFiles">
		<SchemeIndexedColumn Column="c4c0ca80-0b2e-00c1-3100-08d57ff13ed1" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="c4c0ca80-0b2e-00c1-7000-08d57ff13ed1" Name="idx_TaskAttachmentFiles_ID" IsClustered="true">
		<SchemeIndexedColumn Column="c4c0ca80-0b2e-01c1-4000-08d57ff13ed1" />
	</SchemeIndex>
</SchemeTable>