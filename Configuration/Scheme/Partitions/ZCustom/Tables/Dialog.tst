<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="f1204c6c-37ff-4943-9ecb-accb2aa0f80e" Name="Dialog" Group="Custom" InstanceType="Cards" ContentType="Entries">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="f1204c6c-37ff-0043-2000-0ccb2aa0f80e" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="f1204c6c-37ff-0143-4000-0ccb2aa0f80e" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="f347ca7d-4757-4ebd-a421-b58d0490dd77" Name="Name" Type="String(128) Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="f1af4e09-84ca-4e55-a087-23f963a5e4ea" Name="df_Dialog_Name" Value="                                                             Является ли данное действие переносом срока?" />
	</SchemePhysicalColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="f1204c6c-37ff-0043-5000-0ccb2aa0f80e" Name="pk_Dialog" IsClustered="true">
		<SchemeIndexedColumn Column="f1204c6c-37ff-0143-4000-0ccb2aa0f80e" />
	</SchemePrimaryKey>
</SchemeTable>