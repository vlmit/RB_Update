<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="070ee1b3-8745-4886-8d63-6df93d336436" Name="MobileMenu" Group="Custom" InstanceType="Cards" ContentType="Entries">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="070ee1b3-8745-0086-2000-0df93d336436" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="070ee1b3-8745-0186-4000-0df93d336436" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="632f966c-46d1-4834-b5ed-55a06117d8e7" Name="Name" Type="String(128) Null" />
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="070ee1b3-8745-0086-5000-0df93d336436" Name="pk_MobileMenu" IsClustered="true">
		<SchemeIndexedColumn Column="070ee1b3-8745-0186-4000-0df93d336436" />
	</SchemePrimaryKey>
</SchemeTable>