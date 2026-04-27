<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="e1b3cb70-f7fc-4b62-b822-e5302d673356" ID="56cc4eba-5c3c-41b5-afd1-deed95dc739f" Name="PersonalListRoles" Group="Junicsoft" InstanceType="Cards" ContentType="Collections">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="56cc4eba-5c3c-00b5-2000-0eed95dc739f" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="56cc4eba-5c3c-01b5-4000-0eed95dc739f" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="56cc4eba-5c3c-00b5-3100-0eed95dc739f" Name="RowID" Type="Guid Not Null" />
	<SchemeComplexColumn ID="8dd89248-dec8-4fa9-b51b-733e2e983b2c" Name="Role" Type="Reference(Typified) Not Null" ReferencedTable="81f6010b-9641-4aa5-8897-b8e8603fbf4b">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="8dd89248-dec8-00a9-4000-033e2e983b2c" Name="RoleID" Type="Guid Not Null" ReferencedColumn="81f6010b-9641-01a5-4000-08e8603fbf4b" />
		<SchemeReferencingColumn ID="34288220-3257-4c2f-9455-637e26d7f61c" Name="RoleName" Type="String(128) Not Null" ReferencedColumn="616d6b2e-35d5-424d-846b-618eb25962d0" />
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="56cc4eba-5c3c-00b5-5000-0eed95dc739f" Name="pk_PersonalListRoles">
		<SchemeIndexedColumn Column="56cc4eba-5c3c-00b5-3100-0eed95dc739f" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="56cc4eba-5c3c-00b5-7000-0eed95dc739f" Name="idx_PersonalListRoles_ID" IsClustered="true">
		<SchemeIndexedColumn Column="56cc4eba-5c3c-01b5-4000-0eed95dc739f" />
	</SchemeIndex>
</SchemeTable>