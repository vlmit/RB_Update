<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="d1b372f3-7565-4309-9037-5e5a0969d94e" ID="1f805af5-f412-4878-9d70-af989b905fb5" Name="WfResolutionsVirtual" Group="Wf" IsVirtual="true" InstanceType="Tasks" ContentType="Entries">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="1f805af5-f412-0078-2000-0f989b905fb5" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="5bfa9936-bb5a-4e8f-89a9-180bfd8f75f8">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="1f805af5-f412-0178-4000-0f989b905fb5" Name="ID" Type="Guid Not Null" ReferencedColumn="5bfa9936-bb5a-008f-3100-080bfd8f75f8" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="d3722c05-74e9-44f0-a97f-de21afda8a29" Name="Planned" Type="DateTime Null">
		<Description>Дата запланированного завершения задания, которую изменяет автор задачи.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="349f8f49-f29a-4e6f-827d-bb333f86bb1d" Name="Digest" Type="String(Max) Null">
		<Description>Digest, который изменяет автор задачи.</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="3e643231-4e0c-478b-9b56-710f33ec9bc0" Name="Role" Type="Reference(Typified) Not Null" ReferencedTable="81f6010b-9641-4aa5-8897-b8e8603fbf4b">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="3e643231-4e0c-008b-4000-010f33ec9bc0" Name="RoleID" Type="Guid Not Null" ReferencedColumn="81f6010b-9641-01a5-4000-08e8603fbf4b" />
		<SchemeReferencingColumn ID="d0d91c8e-05f3-405f-8daf-5d1c1ec61605" Name="RoleName" Type="String(128) Not Null" ReferencedColumn="616d6b2e-35d5-424d-846b-618eb25962d0">
			<Description>Отображаемое имя роли.</Description>
		</SchemeReferencingColumn>
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="1f805af5-f412-0078-5000-0f989b905fb5" Name="pk_WfResolutionsVirtual" IsClustered="true">
		<SchemeIndexedColumn Column="1f805af5-f412-0178-4000-0f989b905fb5" />
	</SchemePrimaryKey>
</SchemeTable>