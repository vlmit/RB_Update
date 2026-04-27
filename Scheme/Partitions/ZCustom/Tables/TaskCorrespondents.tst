<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="99560120-7746-4241-99e2-e2ddc3d40002" Name="TaskCorrespondents" Group="Custom" InstanceType="Tasks" ContentType="Collections">
	<Description></Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="99560120-7746-0041-2000-02ddc3d40002" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="5bfa9936-bb5a-4e8f-89a9-180bfd8f75f8">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="99560120-7746-0141-4000-02ddc3d40002" Name="ID" Type="Guid Not Null" ReferencedColumn="5bfa9936-bb5a-008f-3100-080bfd8f75f8" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="99560120-7746-0041-3100-02ddc3d40002" Name="RowID" Type="Guid Not Null" />
	<SchemeComplexColumn ID="3134880b-e6d3-42e2-8f8e-b42dd705a523" Name="Partners" Type="Reference(Typified) Null" ReferencedTable="5d47ef13-b6f4-47ef-9815-3b3d0e6d475a">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="3134880b-e6d3-00e2-4000-042dd705a523" Name="PartnersID" Type="Guid Null" ReferencedColumn="5d47ef13-b6f4-01ef-4000-0b3d0e6d475a" />
		<SchemeReferencingColumn ID="6b5a49a2-e05e-44e7-9e91-c26897d9cc3a" Name="PartnersName" Type="String(255) Null" ReferencedColumn="f1c960e0-951e-4837-8474-bb61d98f40f0" />
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="99560120-7746-0041-5000-02ddc3d40002" Name="pk_TaskCorrespondents">
		<SchemeIndexedColumn Column="99560120-7746-0041-3100-02ddc3d40002" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="99560120-7746-0041-7000-02ddc3d40002" Name="idx_TaskCorrespondents_ID" IsClustered="true">
		<SchemeIndexedColumn Column="99560120-7746-0141-4000-02ddc3d40002" />
	</SchemeIndex>
</SchemeTable>