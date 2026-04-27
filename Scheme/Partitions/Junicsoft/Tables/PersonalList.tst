<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="e1b3cb70-f7fc-4b62-b822-e5302d673356" ID="177e70e5-4e7e-41a0-882d-87ea0417a931" Name="PersonalList" Group="Junicsoft" InstanceType="Cards" ContentType="Entries">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="177e70e5-4e7e-00a0-2000-07ea0417a931" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="177e70e5-4e7e-01a0-4000-07ea0417a931" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="be818af4-a3dd-4905-9a99-b808b8a2ad3e" Name="Title" Type="String(128) Not Null" />
	<SchemeComplexColumn ID="ac28c3ff-1dd0-4a3a-943a-a23d61774802" Name="Owner" Type="Reference(Typified) Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="ac28c3ff-1dd0-003a-4000-023d61774802" Name="OwnerID" Type="Guid Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="c27f62cc-2ad2-465d-8ced-cc865808f02e" Name="OwnerName" Type="String(128) Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="948ee1bf-a1ee-46b2-95bf-37bbc068be8c" Name="Status" Type="Boolean Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="40941702-ac43-405c-b0ad-8f2727bee6d6" Name="df_PersonalList_Status" Value="false" />
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="1aa0b2b1-98be-482c-860c-b148064b78e2" Name="LinkedStaticRole" Type="Reference(Typified) Null" ReferencedTable="81f6010b-9641-4aa5-8897-b8e8603fbf4b">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="1aa0b2b1-98be-002c-4000-0148064b78e2" Name="LinkedStaticRoleID" Type="Guid Null" ReferencedColumn="81f6010b-9641-01a5-4000-08e8603fbf4b" />
		<SchemeReferencingColumn ID="d3ed8add-7531-43bd-b59b-c7436434b029" Name="LinkedStaticRoleName" Type="String(128) Null" ReferencedColumn="616d6b2e-35d5-424d-846b-618eb25962d0" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="972b531c-0d53-420f-830e-dd39d404db24" Name="SortT" Type="String(128) Null">
		<Description>Поле для сортироваки (Текстовое)</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="42e89a05-0daa-4dc4-be2a-729d53826be8" Name="SortN" Type="Int16 Null">
		<Description>Поле для сортировки (Числовое)</Description>
	</SchemePhysicalColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="177e70e5-4e7e-00a0-5000-07ea0417a931" Name="pk_PersonalList" IsClustered="true">
		<SchemeIndexedColumn Column="177e70e5-4e7e-01a0-4000-07ea0417a931" />
	</SchemePrimaryKey>
</SchemeTable>